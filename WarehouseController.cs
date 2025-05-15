namespace Tutorial9.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Model.DTOs;
using Tutorial9.Services;


[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IDbService _dbService;

    public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouseAsync([FromBody] ProductWarehouseDTO  productWarehouse)
    {
        try
        {
            if (productWarehouse.Amount <= 0)
                return BadRequest("Amount must be greater than 0");

            var newId = await _dbService.AddProductToWarehouseAsync(productWarehouse);
            return Ok(new { IdProductWarehouse = newId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("procedure")]
    public async Task<IActionResult> ProcedureAsync([FromBody] ProductWarehouseDTO  productWarehouse)
    {
        try
        {
            if (productWarehouse.Amount <= 0)
                return BadRequest("Amount must be greater than 0");

            var newId = await _dbService.ProcedureAsync(productWarehouse);
            return Ok(new { IdProductWarehouse = newId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

}