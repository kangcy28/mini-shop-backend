using Microsoft.EntityFrameworkCore;
using EcommerceAdminAPI.Data;
using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoryAsync()
        {
            return await _dbSet.Include(p => p.Category).ToListAsync();
        }

        public async Task<Product?> GetProductWithCategoryAsync(int id)
        {
            return await _dbSet.Include(p => p.Category)
                              .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet.Where(p => p.CategoryId == categoryId)
                              .Include(p => p.Category)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet.Where(p => p.IsActive)
                              .Include(p => p.Category)
                              .ToListAsync();
        }
    }
}