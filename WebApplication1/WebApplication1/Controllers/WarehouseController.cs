using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }


    [HttpPost]
    public async Task<IActionResult> Post(ProductWarehouseReq req)
    {
        var product = await _warehouseRepository.FindProductById(req.IdProduct);
        if (product is null)
            return BadRequest("Product not found");
        
        var warehouse = await _warehouseRepository.FindWarehouseById(req.IdWarehouse);
        if (warehouse is null)
            return BadRequest("Warehouse not found");
        
        if (req.Amount < 0)
            return BadRequest("Amount < 0");

        Order order = await _warehouseRepository.FindOrderByProductIdAndAmount(req.IdProduct, req.Amount);
        if (order is null)
            return BadRequest("Order not found");

        if (order.FulfilledAt is not null)
            return BadRequest("Order fulfilled");

        await _warehouseRepository.ChangeFulfilledAt(req.IdProduct, req.Amount);
        await _warehouseRepository.InsertProductWarehouse(req);
        
        return Ok(_warehouseRepository.GetResultPrimaryKey(req));
    }
}