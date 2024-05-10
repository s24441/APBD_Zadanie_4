using APBD_Task_6.Models;
using APBD_Zadanie_6.Exceptions;
using System.Data.SqlClient;

namespace Zadanie5.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;

        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> AddProduct(ProductWarehouse productWarehouse)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using var connection = new SqlConnection(connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;
            await cmd.Connection.OpenAsync();
            cmd.CommandText = @"
                SELECT TOP 1 [Order].IdOrder FROM [Order]
                LEFT JOIN Product_Warehouse ON [Order].IdOrder = Product_Warehouse.IdOrder
                WHERE [Order].IdProduct = @IdProduct
                AND [Order].Amount = @Amount
                AND Product_Warehouse.IdProductWarehouse IS NULL
                AND [Order].CreatedAt < @CreatedAt
            ";

            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
            cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount);
            cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);

            var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new NoRowsException();

            await reader.ReadAsync();
            var idOrder = int.Parse(reader["IdOrder"]?.ToString());
            await reader.CloseAsync();

            cmd.Parameters.Clear();

            /////////// POBRANIE CENY ///////////
            cmd.CommandText = @"
                SELECT Price FROM Product WHERE IdProduct = @IdProduct
            ";
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);

            reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new NoRowsException();

            await reader.ReadAsync();
            var price = double.Parse(reader["Price"]?.ToString());
            await reader.CloseAsync();

            cmd.Parameters.Clear();

            /////////// SPRADZENIE CZY WAREHOUSE ISTNIEJE ///////////
            cmd.CommandText = @"
                SELECT IdWarehouse FROM Warehouse WHERE IdWarehouse = @IdWarehouse
            ";
            cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);

            reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new NoRowsException();

            await reader.CloseAsync();

            cmd.Parameters.Clear();

            /////////// TRANZAKCJA UPDATE/INSERT ///////////
            var trans = (SqlTransaction)await connection.BeginTransactionAsync();
            cmd.Transaction = trans;

            try
            {
                cmd.CommandText = @"
                    UPDATE [Order] SET FulfilledAt = @CreatedAt WHERE IdOrder = @IdOrder
                ";
                cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);
                cmd.Parameters.AddWithValue("IdOrder", idOrder);

                var rowsUpdated = await cmd.ExecuteNonQueryAsync();

                if (rowsUpdated < 1) throw new NoRowsUpdatedException();

                cmd.Parameters.Clear();

                cmd.CommandText = @"
                    INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*@Price, @CreatedAt)
                ";
                cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("IdOrder", idOrder);
                cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("Price", price);
                cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);

                var rowsInserted = await cmd.ExecuteNonQueryAsync();

                if (rowsInserted < 1) throw new NoRowsUpdatedException();

                await trans.CommitAsync();
            }
            catch (Exception e) 
            { 
                await trans.RollbackAsync();
                throw e;
            }

            cmd.CommandText = "SELECT TOP 1 IdProductWarehouse FROM Product_Warehouse ORDER BY IdProductWarehouse";
            reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            int idProductWarehouse = int.Parse(reader["IdProductWarehouse"]?.ToString());

            await reader.CloseAsync();
            await connection.CloseAsync();

            return idProductWarehouse;
        }
    }
}
