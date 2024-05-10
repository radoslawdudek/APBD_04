using APBD_04.Models;
using APBD_04.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APBD_04.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseRepository _iWarehouseRepository;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _iWarehouseRepository = warehouseRepository;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductWarehouse product)
    {
        int idProductWarehouse = await _iWarehouseRepository.AddProduct(product);
        return Ok(idProductWarehouse);
    }
    
    [HttpPost ("AddProductToWarehouse")]
    public async Task<IActionResult> AddProductToWarehouse(ProductWarehouse product)
    {
        int idProductWarehouse = await _iWarehouseRepository.AddProductToWarehouse(product);
        return Ok(idProductWarehouse);
    }
    
    
}