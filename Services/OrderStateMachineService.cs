using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Services
{
    public class OrderStateMachineService : IOrderStateMachineService
    {
        public bool CanTransitionTo(Order order, string newStatus)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return GetValidTransitions(order.Status).Contains(newStatus);
        }

        public void TransitionTo(Order order, string newStatus)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanTransitionTo(order, newStatus))
            {
                throw new InvalidOperationException($"Cannot transition from {order.Status} to {newStatus}");
            }

            order.Status = newStatus;
        }

        public IEnumerable<string> GetValidTransitions(string currentStatus)
        {
            return currentStatus switch
            {
                "Pending" => new List<string> { "Paid" },
                "Paid" => new List<string> { "Shipped", "Refunded" },
                "Shipped" => new List<string> { "Completed", "Refunded" },
                "Completed" => new List<string> { "Refunded" },
                "Refunded" => new List<string>(),
                _ => new List<string>()
            };
        }
    }
}