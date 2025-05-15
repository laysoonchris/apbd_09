using Tutorial9.Model;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public interface IDbService
{
    Task<int> AddProductToWarehouseAsync(ProductWarehouseDTO productWarehouse);
    Task<int> ProcedureAsync(ProductWarehouseDTO productWarehouse);
}