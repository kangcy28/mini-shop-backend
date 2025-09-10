using EcommerceAdminAPI.Models;
using FluentAssertions;
using Xunit;

namespace EcommerceAdminAPI.Tests.Services
{
    public class OrderStateMachineTests
    {
        public class OrderStateMachine
        {
            private readonly Order _order;

            public OrderStateMachine(Order order)
            {
                _order = order ?? throw new ArgumentNullException(nameof(order));
            }

            public bool CanTransitionTo(string newStatus)
            {
                return GetValidTransitions(_order.Status).Contains(newStatus);
            }

            public void TransitionTo(string newStatus)
            {
                if (!CanTransitionTo(newStatus))
                {
                    throw new InvalidOperationException($"Cannot transition from {_order.Status} to {newStatus}");
                }
                
                _order.Status = newStatus;
            }

            private static List<string> GetValidTransitions(string currentStatus)
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

        [Fact]
        public void Order_InitialStatus_ShouldBePending()
        {
            var order = new Order
            {
                OrderNumber = "ORD-001",
                TotalAmount = 100.00m
            };

            order.Status.Should().Be("Pending");
        }

        [Theory]
        [InlineData("Pending", "Paid")]
        [InlineData("Paid", "Shipped")]
        [InlineData("Paid", "Refunded")]
        [InlineData("Shipped", "Completed")]
        [InlineData("Shipped", "Refunded")]
        [InlineData("Completed", "Refunded")]
        public void TransitionTo_ValidTransitions_ShouldSucceed(string fromStatus, string toStatus)
        {
            var order = new Order { Status = fromStatus };
            var stateMachine = new OrderStateMachine(order);

            stateMachine.TransitionTo(toStatus);

            order.Status.Should().Be(toStatus);
        }

        [Theory]
        [InlineData("Pending", "Shipped")]
        [InlineData("Pending", "Completed")]
        [InlineData("Pending", "Refunded")]
        [InlineData("Paid", "Completed")]
        [InlineData("Shipped", "Paid")]
        [InlineData("Completed", "Pending")]
        [InlineData("Completed", "Paid")]
        [InlineData("Completed", "Shipped")]
        [InlineData("Refunded", "Pending")]
        [InlineData("Refunded", "Paid")]
        [InlineData("Refunded", "Shipped")]
        [InlineData("Refunded", "Completed")]
        public void TransitionTo_InvalidTransitions_ShouldThrowException(string fromStatus, string toStatus)
        {
            var order = new Order { Status = fromStatus };
            var stateMachine = new OrderStateMachine(order);

            Action action = () => stateMachine.TransitionTo(toStatus);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot transition from {fromStatus} to {toStatus}");
        }

        [Theory]
        [InlineData("Pending", "Paid", true)]
        [InlineData("Paid", "Shipped", true)]
        [InlineData("Paid", "Refunded", true)]
        [InlineData("Shipped", "Completed", true)]
        [InlineData("Shipped", "Refunded", true)]
        [InlineData("Completed", "Refunded", true)]
        [InlineData("Pending", "Shipped", false)]
        [InlineData("Pending", "Completed", false)]
        [InlineData("Refunded", "Pending", false)]
        public void CanTransitionTo_ShouldReturnCorrectResult(string fromStatus, string toStatus, bool expected)
        {
            var order = new Order { Status = fromStatus };
            var stateMachine = new OrderStateMachine(order);

            var result = stateMachine.CanTransitionTo(toStatus);

            result.Should().Be(expected);
        }

        [Fact]
        public void OrderStateMachine_WithNullOrder_ShouldThrowArgumentNullException()
        {
            Action action = () => new OrderStateMachine(null!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("order");
        }

        [Fact]
        public void CompleteOrderWorkflow_ShouldFollowValidPath()
        {
            var order = new Order
            {
                OrderNumber = "ORD-001",
                TotalAmount = 100.00m,
                Status = "Pending"
            };
            var stateMachine = new OrderStateMachine(order);

            stateMachine.TransitionTo("Paid");
            order.Status.Should().Be("Paid");

            stateMachine.TransitionTo("Shipped");
            order.Status.Should().Be("Shipped");

            stateMachine.TransitionTo("Completed");
            order.Status.Should().Be("Completed");
        }

        [Fact]
        public void RefundWorkflow_FromPaidStatus_ShouldWork()
        {
            var order = new Order
            {
                OrderNumber = "ORD-001",
                TotalAmount = 100.00m,
                Status = "Paid"
            };
            var stateMachine = new OrderStateMachine(order);

            stateMachine.TransitionTo("Refunded");
            order.Status.Should().Be("Refunded");

            stateMachine.CanTransitionTo("Pending").Should().BeFalse();
            stateMachine.CanTransitionTo("Paid").Should().BeFalse();
            stateMachine.CanTransitionTo("Shipped").Should().BeFalse();
            stateMachine.CanTransitionTo("Completed").Should().BeFalse();
        }

        [Fact]
        public void RefundWorkflow_FromShippedStatus_ShouldWork()
        {
            var order = new Order
            {
                OrderNumber = "ORD-001",
                TotalAmount = 100.00m,
                Status = "Shipped"
            };
            var stateMachine = new OrderStateMachine(order);

            stateMachine.TransitionTo("Refunded");
            order.Status.Should().Be("Refunded");
        }

        [Fact]
        public void RefundWorkflow_FromCompletedStatus_ShouldWork()
        {
            var order = new Order
            {
                OrderNumber = "ORD-001",
                TotalAmount = 100.00m,
                Status = "Completed"
            };
            var stateMachine = new OrderStateMachine(order);

            stateMachine.TransitionTo("Refunded");
            order.Status.Should().Be("Refunded");
        }
    }
}