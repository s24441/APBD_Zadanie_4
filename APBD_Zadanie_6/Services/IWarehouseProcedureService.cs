using APBD_Task_6.Models;

namespace APBD_Zadanie_6.Services
{
    public interface IWarehouseProcedureService
    {
        Task<int> AddProductToWarehouse(ProductWarehouse productWarehouse);
    }
}
