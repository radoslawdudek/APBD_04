using APBD_04.Models;

namespace APBD_04.Repositories;

public interface IWarehouseRepository
{
    Task<int> AddProduct(ProductWarehouse productWarehouse);
    Task<int> AddProductToWarehouse(ProductWarehouse productWarehouse);
}