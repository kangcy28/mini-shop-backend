using Microsoft.EntityFrameworkCore;
using EcommerceAdminAPI.Data;
using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync()
        {
            return await _dbSet.Include(o => o.OrderDetails)
                              .ThenInclude(od => od.Product)
                              .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            return await _dbSet.Include(o => o.OrderDetails)
                              .ThenInclude(od => od.Product)
                              .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _dbSet.Where(o => o.Status == status)
                              .Include(o => o.OrderDetails)
                              .ThenInclude(od => od.Product)
                              .ToListAsync();
        }
    }
}