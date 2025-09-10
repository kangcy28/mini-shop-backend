using EcommerceAdminAPI.Models;
using EcommerceAdminAPI.Repositories;

namespace EcommerceAdminAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStateMachineService _stateMachineService;

        public OrderService(IOrderRepository orderRepository, IOrderStateMachineService stateMachineService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _stateMachineService = stateMachineService ?? throw new ArgumentNullException(nameof(stateMachineService));
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetOrdersWithDetailsAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _orderRepository.GetOrdersByStatusAsync(status);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.Status = "Pending";
            order.CreatedAt = DateTime.UtcNow;

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Update(order);
            await _orderRepository.SaveAsync();
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return false;

            _orderRepository.Delete(order);
            await _orderRepository.SaveAsync();
            return true;
        }

        public async Task<Order> TransitionOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order with ID {orderId} not found");

            _stateMachineService.TransitionTo(order, newStatus);
            _orderRepository.Update(order);
            await _orderRepository.SaveAsync();
            
            return order;
        }

        public async Task<IEnumerable<string>> GetValidTransitionsForOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order with ID {orderId} not found");

            return _stateMachineService.GetValidTransitions(order.Status);
        }
    }
}