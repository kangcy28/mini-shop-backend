using EcommerceAdminAPI.Models;
using EcommerceAdminAPI.Services;
using EcommerceAdminAPI.Repositories;
using Moq;
using FluentAssertions;
using Xunit;

namespace EcommerceAdminAPI.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderStateMachineService> _mockStateMachineService;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockStateMachineService = new Mock<IOrderStateMachineService>();
            _orderService = new OrderService(_mockOrderRepository.Object, _mockStateMachineService.Object);
        }

        [Fact]
        public void Constructor_WithNullOrderRepository_ShouldThrowArgumentNullException()
        {
            Action action = () => new OrderService(null!, _mockStateMachineService.Object);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("orderRepository");
        }

        [Fact]
        public void Constructor_WithNullStateMachineService_ShouldThrowArgumentNullException()
        {
            Action action = () => new OrderService(_mockOrderRepository.Object, null!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("stateMachineService");
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            var expectedOrders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" },
                new Order { Id = 2, OrderNumber = "ORD-002", Status = "Paid" }
            };
            _mockOrderRepository.Setup(r => r.GetOrdersWithDetailsAsync())
                .ReturnsAsync(expectedOrders);

            var result = await _orderService.GetAllOrdersAsync();

            result.Should().BeEquivalentTo(expectedOrders);
            _mockOrderRepository.Verify(r => r.GetOrdersWithDetailsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithValidId_ShouldReturnOrder()
        {
            var expectedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" };
            _mockOrderRepository.Setup(r => r.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(expectedOrder);

            var result = await _orderService.GetOrderByIdAsync(1);

            result.Should().BeEquivalentTo(expectedOrder);
            _mockOrderRepository.Verify(r => r.GetOrderWithDetailsAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            _mockOrderRepository.Setup(r => r.GetOrderWithDetailsAsync(999))
                .ReturnsAsync((Order?)null);

            var result = await _orderService.GetOrderByIdAsync(999);

            result.Should().BeNull();
            _mockOrderRepository.Verify(r => r.GetOrderWithDetailsAsync(999), Times.Once);
        }

        [Fact]
        public async Task GetOrdersByStatusAsync_WithValidStatus_ShouldReturnFilteredOrders()
        {
            var expectedOrders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" },
                new Order { Id = 3, OrderNumber = "ORD-003", Status = "Pending" }
            };
            _mockOrderRepository.Setup(r => r.GetOrdersByStatusAsync("Pending"))
                .ReturnsAsync(expectedOrders);

            var result = await _orderService.GetOrdersByStatusAsync("Pending");

            result.Should().BeEquivalentTo(expectedOrders);
            _mockOrderRepository.Verify(r => r.GetOrdersByStatusAsync("Pending"), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidOrder_ShouldCreateOrderWithPendingStatus()
        {
            var order = new Order { OrderNumber = "ORD-001", TotalAmount = 100.00m };
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(order);

            var result = await _orderService.CreateOrderAsync(order);

            result.Status.Should().Be("Pending");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            _mockOrderRepository.Verify(r => r.AddAsync(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WithNullOrder_ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () => await _orderService.CreateOrderAsync(null!);

            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("order");
        }

        [Fact]
        public async Task UpdateOrderAsync_WithValidOrder_ShouldUpdateOrder()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Paid" };

            var result = await _orderService.UpdateOrderAsync(order);

            result.Should().BeEquivalentTo(order);
            _mockOrderRepository.Verify(r => r.Update(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithNullOrder_ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () => await _orderService.UpdateOrderAsync(null!);

            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("order");
        }

        [Fact]
        public async Task DeleteOrderAsync_WithExistingOrder_ShouldDeleteAndReturnTrue()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001" };
            _mockOrderRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);

            var result = await _orderService.DeleteOrderAsync(1);

            result.Should().BeTrue();
            _mockOrderRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockOrderRepository.Verify(r => r.Delete(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithNonExistingOrder_ShouldReturnFalse()
        {
            _mockOrderRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Order?)null);

            var result = await _orderService.DeleteOrderAsync(999);

            result.Should().BeFalse();
            _mockOrderRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
            _mockOrderRepository.Verify(r => r.Delete(It.IsAny<Order>()), Times.Never);
            _mockOrderRepository.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task TransitionOrderStatusAsync_WithExistingOrder_ShouldTransitionStatusAndSave()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" };
            _mockOrderRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);
            _mockStateMachineService.Setup(s => s.TransitionTo(order, "Paid"));

            var result = await _orderService.TransitionOrderStatusAsync(1, "Paid");

            result.Should().BeEquivalentTo(order);
            _mockOrderRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockStateMachineService.Verify(s => s.TransitionTo(order, "Paid"), Times.Once);
            _mockOrderRepository.Verify(r => r.Update(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task TransitionOrderStatusAsync_WithNonExistingOrder_ShouldThrowInvalidOperationException()
        {
            _mockOrderRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Order?)null);

            Func<Task> action = async () => await _orderService.TransitionOrderStatusAsync(999, "Paid");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Order with ID 999 not found");
            _mockOrderRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
            _mockStateMachineService.Verify(s => s.TransitionTo(It.IsAny<Order>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TransitionOrderStatusAsync_WhenStateMachineThrowsException_ShouldPropagateException()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" };
            _mockOrderRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);
            _mockStateMachineService.Setup(s => s.TransitionTo(order, "Completed"))
                .Throws(new InvalidOperationException("Cannot transition from Pending to Completed"));

            Func<Task> action = async () => await _orderService.TransitionOrderStatusAsync(1, "Completed");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot transition from Pending to Completed");
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
            _mockOrderRepository.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetValidTransitionsForOrderAsync_WithExistingOrder_ShouldReturnValidTransitions()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" };
            var expectedTransitions = new List<string> { "Paid" };
            _mockOrderRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);
            _mockStateMachineService.Setup(s => s.GetValidTransitions("Pending"))
                .Returns(expectedTransitions);

            var result = await _orderService.GetValidTransitionsForOrderAsync(1);

            result.Should().BeEquivalentTo(expectedTransitions);
            _mockOrderRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockStateMachineService.Verify(s => s.GetValidTransitions("Pending"), Times.Once);
        }

        [Fact]
        public async Task GetValidTransitionsForOrderAsync_WithNonExistingOrder_ShouldThrowInvalidOperationException()
        {
            _mockOrderRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Order?)null);

            Func<Task> action = async () => await _orderService.GetValidTransitionsForOrderAsync(999);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Order with ID 999 not found");
            _mockOrderRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
            _mockStateMachineService.Verify(s => s.GetValidTransitions(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Pending")]
        [InlineData("Paid")]
        [InlineData("Shipped")]
        [InlineData("Completed")]
        [InlineData("Refunded")]
        public async Task GetOrdersByStatusAsync_WithVariousStatuses_ShouldCallRepositoryWithCorrectStatus(string status)
        {
            var expectedOrders = new List<Order>();
            _mockOrderRepository.Setup(r => r.GetOrdersByStatusAsync(status))
                .ReturnsAsync(expectedOrders);

            var result = await _orderService.GetOrdersByStatusAsync(status);

            result.Should().BeEquivalentTo(expectedOrders);
            _mockOrderRepository.Verify(r => r.GetOrdersByStatusAsync(status), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldSetCreatedAtToCurrentTime()
        {
            var order = new Order { OrderNumber = "ORD-001", TotalAmount = 100.00m };
            var beforeCreate = DateTime.UtcNow;
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(order);

            var result = await _orderService.CreateOrderAsync(order);
            var afterCreate = DateTime.UtcNow;

            result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
            result.CreatedAt.Should().BeOnOrBefore(afterCreate);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldNotModifyOriginalOrderObject()
        {
            var originalOrder = new Order { OrderNumber = "ORD-001", TotalAmount = 100.00m, Status = "Custom" };
            var originalCreatedAt = originalOrder.CreatedAt;
            
            _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(originalOrder);

            await _orderService.CreateOrderAsync(originalOrder);

            originalOrder.Status.Should().Be("Pending");
            originalOrder.CreatedAt.Should().NotBe(originalCreatedAt);
        }
    }
}