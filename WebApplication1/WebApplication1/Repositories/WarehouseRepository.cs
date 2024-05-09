using System.Data.SqlClient;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;

    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    
    public async Task<Product> FindProductById(int id)
    {
        var query = "SELECT 1 FROM Product WHERE ID = @ID";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        SqlDataReader dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            return new Product()
            {
                IdProduct = Convert.ToInt32(dataReader["IdProduct"]),
                Name = Convert.ToString(dataReader["Name"]),
                Description = Convert.ToString(dataReader["Description"]),
                Price = Convert.ToDecimal(dataReader["Price"])
            };
        }

        return null;
    }

    public async Task<Warehouse> FindWarehouseById(int id)
    {
        var query = "SELECT 1 FROM Warehouse WHERE ID = @ID";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        SqlDataReader dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            return new Warehouse()
            {
                IdWarehouse = Convert.ToInt32(dataReader["IdWarehouse"]),
                Name = Convert.ToString(dataReader["Name"]),
                Adress = Convert.ToString(dataReader["Adress"])
            };
        }

        return null;
    }

    public async Task<Order> FindOrderByProductIdAndAmount(int id, int amount)
    {
        var query = "SELECT * FROM Order WHERE IdProduct = @ID AND Amount = @Amount";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Amount", amount);

        await connection.OpenAsync();

        SqlDataReader dataReader = await command.ExecuteReaderAsync();

        if (await dataReader.ReadAsync())
        {
            return new Order()
            {
                IdOrder = Convert.ToInt32(dataReader["IdOrder"]),
                IdProduct = Convert.ToInt32(dataReader["IdProduct"]),
                Amount = Convert.ToInt32(dataReader["Amount"]),
                CreatedAt = Convert.ToDateTime(dataReader["CreatedAt"]),
                FulfilledAt = Convert.ToDateTime(dataReader["FulfiledAt"])
            };
        }

        return null;
    }

    public async Task ChangeFulfilledAt(int id, int amount)
    {
        DateTime date = DateTime.Now;
        var query = "UPDATE Order SET FulfilledAt = @Date WHERE IdProduct = @IdProduct AND Amount = @Amount";
        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@Date", date);

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertProductWarehouse(ProductWarehouseReq req)
    {
        var priceQuery = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
        
        var query = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)" +
                    "VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @Date)";
        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        using SqlCommand priceCommand = new SqlCommand();

        priceCommand.Connection = connection;
        priceCommand.CommandText = priceQuery;
        priceCommand.Parameters.AddWithValue("@IdProduct", req.IdProduct);
        await connection.OpenAsync();
        var price = await priceCommand.ExecuteScalarAsync();
        float.TryParse(price.ToString(), out float p);
        
        
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdWarehouse", req.IdWarehouse);
        command.Parameters.AddWithValue("@IdProduct", req.IdProduct);
        command.Parameters.AddWithValue("@IdOrder", FindOrderByProductIdAndAmount(req.IdProduct, req.Amount));
        command.Parameters.AddWithValue("@Amount", req.Amount);
        command.Parameters.AddWithValue("@Price", p * req.Amount);
        command.Parameters.AddWithValue("@Date", DateTime.Now);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> GetResultPrimaryKey(ProductWarehouseReq req)
    {
        var query = "SELECT IdProductWarehouse FROM Product_Warehouse WHERE IdWarehouse = @IdWarehouse " +
                         "AND IdProduct = @IdProduct AND IdOrder = @IdOrder";
        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdWarehouse", req.IdWarehouse);
        command.Parameters.AddWithValue("@IdProduct", req.IdProduct);
        command.Parameters.AddWithValue("@IdOrder", FindOrderByProductIdAndAmount(req.IdProduct, req.Amount));

        await connection.OpenAsync();

        var res = command.ExecuteScalarAsync();
        int.TryParse(res.ToString(), out int id);

        return id;
    }
}