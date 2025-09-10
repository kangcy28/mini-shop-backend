using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Services
{
    public interface IOrderStateMachineService
    {
        bool CanTransitionTo(Order order, string newStatus);
        void TransitionTo(Order order, string newStatus);
        IEnumerable<string> GetValidTransitions(string currentStatus);
    }
}