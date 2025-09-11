using Microsoft.AspNetCore.Mvc;
using EcommerceAdminAPI.Controllers;
using EcommerceAdminAPI.Models;
using EcommerceAdminAPI.Services;
using Moq;
using FluentAssertions;
using Xunit;

namespace EcommerceAdminAPI.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrdersController(_mockOrderService.Object);
        }

        [Fact]
        public void Constructor_WithNullOrderService_ShouldThrowArgumentNullException()
        {
            Action action = () => new OrdersController(null!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("orderService");
        }

        #region GetOrders Tests

        [Fact]
        public async Task GetOrders_ShouldReturnOkWithAllOrders()
        {
            var expectedOrders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" },
                new Order { Id = 2, OrderNumber = "ORD-002", Status = "Paid" }
            };
            _mockOrderService.Setup(s => s.GetAllOrdersAsync())
                .ReturnsAsync(expectedOrders);

            var result = await _controller.GetOrders();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedOrders);
            _mockOrderService.Verify(s => s.GetAllOrdersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOrders_WithEmptyList_ShouldReturnOkWithEmptyList()
        {
            var emptyOrders = new List<Order>();
            _mockOrderService.Setup(s => s.GetAllOrdersAsync())
                .ReturnsAsync(emptyOrders);

            var result = await _controller.GetOrders();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(emptyOrders);
        }

        #endregion

        #region GetOrder Tests

        [Fact]
        public async Task GetOrder_WithExistingId_ShouldReturnOkWithOrder()
        {
            var expectedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" };
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(1))
                .ReturnsAsync(expectedOrder);

            var result = await _controller.GetOrder(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedOrder);
            _mockOrderService.Verify(s => s.GetOrderByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetOrder_WithNonExistingId_ShouldReturnNotFound()
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(999))
                .ReturnsAsync((Order?)null);

            var result = await _controller.GetOrder(999);

            result.Result.Should().BeOfType<NotFoundResult>();
            _mockOrderService.Verify(s => s.GetOrderByIdAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task GetOrder_WithInvalidId_ShouldReturnNotFound(int invalidId)
        {
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(invalidId))
                .ReturnsAsync((Order?)null);

            var result = await _controller.GetOrder(invalidId);

            result.Result.Should().BeOfType<NotFoundResult>();
            _mockOrderService.Verify(s => s.GetOrderByIdAsync(invalidId), Times.Once);
        }

        #endregion

        #region GetOrdersByStatus Tests

        [Fact]
        public async Task GetOrdersByStatus_WithValidStatus_ShouldReturnOkWithFilteredOrders()
        {
            var expectedOrders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD-001", Status = "Pending" },
                new Order { Id = 3, OrderNumber = "ORD-003", Status = "Pending" }
            };
            _mockOrderService.Setup(s => s.GetOrdersByStatusAsync("Pending"))
                .ReturnsAsync(expectedOrders);

            var result = await _controller.GetOrdersByStatus("Pending");

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedOrders);
            _mockOrderService.Verify(s => s.GetOrdersByStatusAsync("Pending"), Times.Once);
        }

        [Theory]
        [InlineData("Pending")]
        [InlineData("Paid")]
        [InlineData("Shipped")]
        [InlineData("Completed")]
        [InlineData("Refunded")]
        public async Task GetOrdersByStatus_WithVariousStatuses_ShouldCallServiceWithCorrectStatus(string status)
        {
            var orders = new List<Order>();
            _mockOrderService.Setup(s => s.GetOrdersByStatusAsync(status))
                .ReturnsAsync(orders);

            await _controller.GetOrdersByStatus(status);

            _mockOrderService.Verify(s => s.GetOrdersByStatusAsync(status), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("InvalidStatus")]
        public async Task GetOrdersByStatus_WithInvalidStatus_ShouldStillCallService(string invalidStatus)
        {
            var emptyOrders = new List<Order>();
            _mockOrderService.Setup(s => s.GetOrdersByStatusAsync(invalidStatus))
                .ReturnsAsync(emptyOrders);

            var result = await _controller.GetOrdersByStatus(invalidStatus);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(emptyOrders);
            _mockOrderService.Verify(s => s.GetOrdersByStatusAsync(invalidStatus), Times.Once);
        }

        #endregion

        #region CreateOrder Tests

        [Fact]
        public async Task CreateOrder_WithValidOrder_ShouldReturnCreatedAtAction()
        {
            var order = new Order { OrderNumber = "ORD-001", TotalAmount = 100.00m };
            var createdOrder = new Order { Id = 1, OrderNumber = "ORD-001", TotalAmount = 100.00m, Status = "Pending" };
            _mockOrderService.Setup(s => s.CreateOrderAsync(order))
                .ReturnsAsync(createdOrder);

            var result = await _controller.CreateOrder(order);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetOrder));
            createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(1);
            createdResult.Value.Should().BeEquivalentTo(createdOrder);
            _mockOrderService.Verify(s => s.CreateOrderAsync(order), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_WithInvalidModelState_ShouldReturnBadRequest()
        {
            _controller.ModelState.AddModelError("OrderNumber", "OrderNumber is required");

            var result = await _controller.CreateOrder(new Order());

            result.Result.Should().BeOfType<BadRequestObjectResult>();
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_WhenServiceThrowsException_ShouldThrow()
        {
            var order = new Order { OrderNumber = "ORD-001", TotalAmount = 100.00m };
            _mockOrderService.Setup(s => s.CreateOrderAsync(order))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var action = async () => await _controller.CreateOrder(order);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }

        #endregion

        #region UpdateOrder Tests

        [Fact]
        public async Task UpdateOrder_WithValidOrder_ShouldReturnNoContent()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Paid" };
            _mockOrderService.Setup(s => s.UpdateOrderAsync(order))
                .ReturnsAsync(order);

            var result = await _controller.UpdateOrder(1, order);

            result.Should().BeOfType<NoContentResult>();
            _mockOrderService.Verify(s => s.UpdateOrderAsync(order), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_WithMismatchedId_ShouldReturnBadRequest()
        {
            var order = new Order { Id = 2, OrderNumber = "ORD-001" };

            var result = await _controller.UpdateOrder(1, order);

            result.Should().BeOfType<BadRequestResult>();
            _mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_WithInvalidModelState_ShouldReturnBadRequest()
        {
            var order = new Order { Id = 1 };
            _controller.ModelState.AddModelError("OrderNumber", "OrderNumber is required");

            var result = await _controller.UpdateOrder(1, order);

            result.Should().BeOfType<BadRequestObjectResult>();
            _mockOrderService.Verify(s => s.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_WhenServiceThrowsInvalidOperationException_ShouldReturnNotFound()
        {
            var order = new Order { Id = 1, OrderNumber = "ORD-001" };
            _mockOrderService.Setup(s => s.UpdateOrderAsync(order))
                .ThrowsAsync(new InvalidOperationException("Order not found"));

            var result = await _controller.UpdateOrder(1, order);

            result.Should().BeOfType<NotFoundResult>();
            _mockOrderService.Verify(s => s.UpdateOrderAsync(order), Times.Once);
        }

        #endregion

        #region DeleteOrder Tests

        [Fact]
        public async Task DeleteOrder_WithExistingOrder_ShouldReturnNoContent()
        {
            _mockOrderService.Setup(s => s.DeleteOrderAsync(1))
                .ReturnsAsync(true);

            var result = await _controller.DeleteOrder(1);

            result.Should().BeOfType<NoContentResult>();
            _mockOrderService.Verify(s => s.DeleteOrderAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_WithNonExistingOrder_ShouldReturnNotFound()
        {
            _mockOrderService.Setup(s => s.DeleteOrderAsync(999))
                .ReturnsAsync(false);

            var result = await _controller.DeleteOrder(999);

            result.Should().BeOfType<NotFoundResult>();
            _mockOrderService.Verify(s => s.DeleteOrderAsync(999), Times.Once);
        }

        #endregion

        #region TransitionOrderStatus Tests

        [Fact]
        public async Task TransitionOrderStatus_WithValidRequest_ShouldReturnOkWithUpdatedOrder()
        {
            var request = new TransitionRequest { NewStatus = "Paid" };
            var updatedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Paid" };
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, "Paid"))
                .ReturnsAsync(updatedOrder);

            var result = await _controller.TransitionOrderStatus(1, request);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(updatedOrder);
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, "Paid"), Times.Once);
        }

        [Fact]
        public async Task TransitionOrderStatus_WithNullRequest_ShouldReturnBadRequest()
        {
            var result = await _controller.TransitionOrderStatus(1, null!);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("NewStatus is required");
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task TransitionOrderStatus_WithInvalidNewStatus_ShouldReturnBadRequest(string invalidStatus)
        {
            var request = new TransitionRequest { NewStatus = invalidStatus };

            var result = await _controller.TransitionOrderStatus(1, request);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("NewStatus is required");
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TransitionOrderStatus_WhenServiceThrowsInvalidOperationException_ShouldReturnBadRequest()
        {
            var request = new TransitionRequest { NewStatus = "Completed" };
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, "Completed"))
                .ThrowsAsync(new InvalidOperationException("Cannot transition from Pending to Completed"));

            var result = await _controller.TransitionOrderStatus(1, request);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cannot transition from Pending to Completed");
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, "Completed"), Times.Once);
        }

        [Theory]
        [InlineData("Paid")]
        [InlineData("Shipped")]
        [InlineData("Completed")]
        [InlineData("Refunded")]
        public async Task TransitionOrderStatus_WithVariousStatuses_ShouldCallServiceWithCorrectStatus(string newStatus)
        {
            var request = new TransitionRequest { NewStatus = newStatus };
            var order = new Order { Id = 1, Status = newStatus };
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, newStatus))
                .ReturnsAsync(order);

            await _controller.TransitionOrderStatus(1, request);

            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, newStatus), Times.Once);
        }

        #endregion

        #region GetValidTransitions Tests

        [Fact]
        public async Task GetValidTransitions_WithExistingOrder_ShouldReturnOkWithTransitions()
        {
            var expectedTransitions = new List<string> { "Paid" };
            _mockOrderService.Setup(s => s.GetValidTransitionsForOrderAsync(1))
                .ReturnsAsync(expectedTransitions);

            var result = await _controller.GetValidTransitions(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedTransitions);
            _mockOrderService.Verify(s => s.GetValidTransitionsForOrderAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetValidTransitions_WithNonExistingOrder_ShouldReturnNotFound()
        {
            _mockOrderService.Setup(s => s.GetValidTransitionsForOrderAsync(999))
                .ThrowsAsync(new InvalidOperationException("Order with ID 999 not found"));

            var result = await _controller.GetValidTransitions(999);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("Order with ID 999 not found");
            _mockOrderService.Verify(s => s.GetValidTransitionsForOrderAsync(999), Times.Once);
        }

        [Fact]
        public async Task GetValidTransitions_WithRefundedOrder_ShouldReturnEmptyTransitions()
        {
            var emptyTransitions = new List<string>();
            _mockOrderService.Setup(s => s.GetValidTransitionsForOrderAsync(1))
                .ReturnsAsync(emptyTransitions);

            var result = await _controller.GetValidTransitions(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(emptyTransitions);
        }

        #endregion

        #region Integration Scenarios Tests

        [Fact]
        public async Task CompleteOrderWorkflow_ShouldWorkThroughAllStates()
        {
            // Setup expected orders for each state
            var paidOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Paid" };
            var shippedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Shipped" };
            var completedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Completed" };
            
            // Setup transitions
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, "Paid"))
                .ReturnsAsync(paidOrder);
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, "Shipped"))
                .ReturnsAsync(shippedOrder);
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, "Completed"))
                .ReturnsAsync(completedOrder);

            // Test Pending -> Paid transition
            var paidResult = await _controller.TransitionOrderStatus(1, new TransitionRequest { NewStatus = "Paid" });
            var paidOkResult = paidResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPaidOrder = paidOkResult.Value.Should().BeOfType<Order>().Subject;
            returnedPaidOrder.Status.Should().Be("Paid");
            returnedPaidOrder.OrderNumber.Should().Be("ORD-001");

            // Test Paid -> Shipped transition
            var shippedResult = await _controller.TransitionOrderStatus(1, new TransitionRequest { NewStatus = "Shipped" });
            var shippedOkResult = shippedResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedShippedOrder = shippedOkResult.Value.Should().BeOfType<Order>().Subject;
            returnedShippedOrder.Status.Should().Be("Shipped");
            returnedShippedOrder.OrderNumber.Should().Be("ORD-001");

            // Test Shipped -> Completed transition
            var completedResult = await _controller.TransitionOrderStatus(1, new TransitionRequest { NewStatus = "Completed" });
            var completedOkResult = completedResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCompletedOrder = completedOkResult.Value.Should().BeOfType<Order>().Subject;
            returnedCompletedOrder.Status.Should().Be("Completed");
            returnedCompletedOrder.OrderNumber.Should().Be("ORD-001");

            // Verify all service calls were made exactly once
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, "Paid"), Times.Once);
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, "Shipped"), Times.Once);
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, "Completed"), Times.Once);
        }

        [Fact] 
        public async Task RefundWorkflow_FromPaidStatus_ShouldWork()
        {
            var refundedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = "Refunded" };
            _mockOrderService.Setup(s => s.TransitionOrderStatusAsync(1, "Refunded"))
                .ReturnsAsync(refundedOrder);

            var result = await _controller.TransitionOrderStatus(1, new TransitionRequest { NewStatus = "Refunded" });

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrder = okResult.Value.Should().BeOfType<Order>().Subject;
            returnedOrder.Status.Should().Be("Refunded");
            returnedOrder.OrderNumber.Should().Be("ORD-001");
            _mockOrderService.Verify(s => s.TransitionOrderStatusAsync(1, "Refunded"), Times.Once);
        }

        #endregion
    }
}