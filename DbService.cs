using System.Data;
using System.Data.Common;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddProductToWarehouseAsync(ProductWarehouseDTO productWarehouse)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        // BEGIN TRANSACTION
        try
        {
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);

            var priceObject = await command.ExecuteScalarAsync();
            if (priceObject == null)
                throw new Exception("Product does not exist");
            
            decimal productPrice = Convert.ToDecimal(priceObject);
            command.Parameters.Clear();
            
            command.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
            
            var warehouseObject = await command.ExecuteScalarAsync();
            if (warehouseObject == null)
                throw new Exception("Warehouse does not exist");
            
            command.Parameters.Clear();
            
            command.CommandText = @"SELECT TOP 1 IdOrder FROM [Order] 
                        WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";

            command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
            
            var orderObject = await command.ExecuteScalarAsync();
            if (orderObject == null)
                throw new Exception("Order does not exist");
            
            int orderId = (int)(decimal)(orderObject);
            command.Parameters.Clear();
            
            command.CommandText = "SELECT 1 FROM ProductWarehouse WHERE IdOrder = @IdOrder";
            
            command.Parameters.AddWithValue("@IdOrder", orderId);
            
            var fulfilled = await command.ExecuteScalarAsync();
            if (fulfilled != null)
                throw new Exception("Product already fulfilled");
            
            command.Parameters.Clear();
            
            command.CommandText = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @IdOrder";
            
            command.Parameters.AddWithValue("@IdOrder", orderId);
            
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            
            decimal totalPrice = productPrice * productWarehouse.Amount;

            command.CommandText = @"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, GETDATE());
                SELECT SCOPE_IDENTITY();
            ";

            command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            command.Parameters.AddWithValue("@Price", totalPrice);
            
            var newIDObject = await command.ExecuteScalarAsync();
            int newID = (int)(decimal)(newIDObject);

            await transaction.CommitAsync();
            return newID;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> ProcedureAsync(ProductWarehouseDTO productWarehouse)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
        command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
        command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
        command.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
        
        var result = await command.ExecuteScalarAsync();
        
        return Convert.ToInt32(result);
    }
}