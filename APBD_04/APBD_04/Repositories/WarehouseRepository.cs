using System.Data;
using System.Data.SqlClient;
using APBD_04.Models;


namespace APBD_04.Repositories;

public class WarehouseRepository : IWarehouseRepository
{

    private readonly IConfiguration _configuration;

    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddProduct(ProductWarehouse productWarehouse)
    {
        var connectionString = _configuration["ConnectionString"];
        await using var connection = new SqlConnection(connectionString);
        await using var cmd = new SqlCommand();

        cmd.Connection = connection;

        await connection.OpenAsync();
        cmd.CommandText = "SELECT TOP 1 [Order].IdOrder FROM [Order] " +
                          "LEFT JOIN Product_Warehouse ON [Order].IdOrder = Product_Warehouse.IdOrder " +
                          "WHERE [Order].IdProduct = @idProduct " +
                          "AND [Order].Amount = @Amount " +
                          "AND Product_Warehouse.IdProductWarehouse IS NULL " +
                          "AND [Order].CreatedAt < @CreatedAt";
        cmd.Parameters.AddWithValue("idProduct", productWarehouse.IdProduct);
        cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount);
        cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);
        var reader = await cmd.ExecuteReaderAsync();

        await reader.ReadAsync();
        if (!reader.HasRows) throw new Exception();

        int idOrder = reader.GetInt32("IdOrder");
        await reader.CloseAsync();
        cmd.Parameters.Clear();

        cmd.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
        cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
        reader = await cmd.ExecuteReaderAsync();


        await reader.ReadAsync();
        decimal price = reader.GetDecimal("Price");
        await reader.CloseAsync();

        cmd.Parameters.Clear();

        cmd.CommandText = "SELECT IdWarehouse FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);
        reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) throw new Exception();
        await reader.CloseAsync();
        cmd.Parameters.Clear();
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();
        cmd.Transaction = transaction;

        try
        {
            cmd.CommandText = "UPDATE [Order] SET FulfilledAt = @CreatedAt WHERE IdOrder = @IdOrder";
            cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);
            cmd.Parameters.AddWithValue("IdOrder", idOrder);
            int rowsUpdated = await cmd.ExecuteNonQueryAsync();

            if (rowsUpdated < 1) throw new Exception();

            cmd.Parameters.Clear();

            cmd.CommandText =
                "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) output INSERTED.IdProductWarehouse " +
                "VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*@Price, @CreatedAt)";
            cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
            cmd.Parameters.AddWithValue("IdOrder", idOrder);
            cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount);
            cmd.Parameters.AddWithValue("Price", price);
            cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);
            int IdProductWarehouse = (int)await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();

            return IdProductWarehouse;
        }
        catch (Exception)
        {
            transaction.RollbackAsync();
            throw;
        }

    }

    public async Task<int> AddProductToWarehouse(ProductWarehouse productWarehouse)
    {
        var connectionString = _configuration["ConnectionString"];
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(); 

        await using (SqlCommand cmd = new SqlCommand("AddProductToWarehouse", connection))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct); 
            cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse); 
            cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount); 
            cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);

            var result = (decimal)await cmd.ExecuteScalarAsync();
            return decimal.ToInt32(result);
        }
    }
}
