using APBD_Task_6.Models;
using APBD_Zadanie_6.Exceptions;
using System.Data;
using System.Data.SqlClient;

namespace APBD_Zadanie_6.Services
{
    public class WarehouseProcedureService : IWarehouseProcedureService
    {
        private readonly IConfiguration _configuration;

        public WarehouseProcedureService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> AddProductToWarehouse(ProductWarehouse productWarehouse)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using var connection = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("AddProductToWarehouse", connection);

            await connection.OpenAsync();

            var trans = (SqlTransaction)await connection.BeginTransactionAsync();
            cmd.Transaction = trans;

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);

                int rowsChanged = await cmd.ExecuteNonQueryAsync();

                if (rowsChanged < 1) throw new NoRowsUpdatedException();

                await trans.CommitAsync();
            }
            catch (Exception ex) 
            { 
                await trans.RollbackAsync();
                throw;
            }

            cmd.CommandText = "SELECT TOP 1 IdProductWarehouse FROM Product_Warehouse ORDER BY IdProductWarehouse";
            cmd.CommandType = CommandType.Text;
            var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            int idProductWarehouse = int.Parse(reader["IdProductWarehouse"]?.ToString());

            await reader.CloseAsync();
            await connection.CloseAsync();

            return idProductWarehouse;
        }
    }
}
