using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using EcommerceAdminAPI.Controllers;
using EcommerceAdminAPI.Services;
using EcommerceAdminAPI.DTOs;

namespace EcommerceAdminAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _authController = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var expectedResponse = new AuthResponseDto
            {
                Token = "jwt-token-here",
                Username = "testuser",
                Email = "test@example.com",
                Role = "User",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
            
            _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "invalid@example.com",
                Password = "wrongpassword"
            };

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            
            var responseValue = unauthorizedResult!.Value;
            responseValue.Should().NotBeNull();
            
            // Using reflection to check the anonymous object property
            var messageProperty = responseValue!.GetType().GetProperty("message");
            messageProperty!.GetValue(responseValue).Should().Be("Invalid email or password");
            
            _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
        }


        [Fact]
        public async Task Login_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var expectedException = new Exception("Database connection failed");
            
            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(expectedException);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            
            var responseValue = objectResult.Value;
            var messageProperty = responseValue!.GetType().GetProperty("message");
            var errorProperty = responseValue.GetType().GetProperty("error");
            
            messageProperty!.GetValue(responseValue).Should().Be("An error occurred during login");
            errorProperty!.GetValue(responseValue).Should().Be("Database connection failed");
            
            _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
        }

        [Theory]
        [InlineData("user@test.com", "validpass123")]
        [InlineData("admin@company.com", "securepassword")]
        [InlineData("customer@store.com", "mypassword")]
        public async Task Login_WithVariousValidCredentials_CallsAuthServiceCorrectly(string email, string password)
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = email,
                Password = password
            };

            var expectedResponse = new AuthResponseDto
            {
                Token = "some-jwt-token",
                Username = "user",
                Email = email,
                Role = "User",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _mockAuthService
                .Setup(x => x.LoginAsync(It.Is<LoginDto>(l => l.Email == email && l.Password == password)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mockAuthService.Verify(x => x.LoginAsync(It.Is<LoginDto>(l => l.Email == email && l.Password == password)), Times.Once);
        }

        [Fact]
        public async Task Login_WithNullLoginDto_ShouldHandleGracefully()
        {
            // Arrange
            LoginDto? loginDto = null;

            // Act & Assert
            // This would typically be handled by model binding in ASP.NET Core
            // but we can test the service call behavior
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authController.Login(loginDto!));
        }

        [Fact]
        public async Task Login_WithEmptyEmail_AuthServiceStillCalled()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "",
                Password = "password123"
            };

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_AuthServiceStillCalled()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = ""
            };

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
        }
    }
}