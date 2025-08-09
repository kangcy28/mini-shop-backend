using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
    }
}