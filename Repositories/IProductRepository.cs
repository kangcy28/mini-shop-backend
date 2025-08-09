using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<Product?> GetProductWithCategoryAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
    }
}