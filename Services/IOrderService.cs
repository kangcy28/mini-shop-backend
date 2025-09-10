using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
        Task<Order> TransitionOrderStatusAsync(int orderId, string newStatus);
        Task<IEnumerable<string>> GetValidTransitionsForOrderAsync(int orderId);
    }
}