using APBD_Task_6.Models;
using APBD_Zadanie_6.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Zadanie_6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesProcedureController : Controller
    {
        private readonly IWarehouseProcedureService _warehouseProcedureService;
        public WarehousesProcedureController(IWarehouseProcedureService warehouseProcedureService) 
        { 
            _warehouseProcedureService = warehouseProcedureService;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse(ProductWarehouse productWarehouse)
        {
            var result = await _warehouseProcedureService.AddProductToWarehouse(productWarehouse);
            return Ok(result);
        }
    }
}
