using APBD_Task_6.Models;
using APBD_Zadanie_6.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Zadanie_6.Controllers
{
    public class WarehouseProcedureController : Controller
    {
        private readonly IWarehouseProcedureService _warehouseProcedureService;
        public WarehouseProcedureController(IWarehouseProcedureService warehouseProcedureService) 
        { 
            _warehouseProcedureService = warehouseProcedureService;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse(ProductWarehouse productWarehouse)
        {
            var result = _warehouseProcedureService.AddProductToWarehouse(productWarehouse);
            return Ok(result);
        }
    }
}
