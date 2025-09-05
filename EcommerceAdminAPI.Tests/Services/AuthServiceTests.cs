using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;
using EcommerceAdminAPI.Services;
using EcommerceAdminAPI.DTOs;
using EcommerceAdminAPI.Models;
using EcommerceAdminAPI.Repositories;
using System.Linq.Expressions;

namespace EcommerceAdminAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IGenericRepository<User>> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IGenericRepository<User>>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup JWT configuration
            var mockJwtSection = new Mock<IConfigurationSection>();
            mockJwtSection.Setup(x => x["Key"]).Returns("ThisIsAVerySecretKeyForTestingPurposesOnly123456789");
            mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
            mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
            var mockExpirationInHoursSection = new Mock<IConfigurationSection>();
            mockExpirationInHoursSection.Setup(x => x.Value).Returns("24");
            mockJwtSection.Setup(x => x.GetSection("ExpirationInHours")).Returns(mockExpirationInHoursSection.Object);
            
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(mockJwtSection.Object);
            
            var mockExpirationSection = new Mock<IConfigurationSection>();
            mockExpirationSection.Setup(x => x.Value).Returns("24");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings:ExpirationInHours")).Returns(mockExpirationSection.Object);
            
            _authService = new AuthService(_mockUserRepository.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ReturnsNull()
        {
            // Arrange
            var inactiveUser = new User
            {
                Id = 1,
                Email = "inactive@example.com",
                Username = "inactive",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "User",
                IsActive = false,  // 設定為非活躍
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new[] { inactiveUser });

            var loginDto = new LoginDto
            {
                Email = "inactive@example.com",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithActiveUserAndValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var activeUser = new User
            {
                Id = 1,
                Email = "active@example.com",
                Username = "activeuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "User",
                IsActive = true,  // 設定為活躍
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new[] { activeUser });

            var loginDto = new LoginDto
            {
                Email = "active@example.com",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("active@example.com");
            result.Username.Should().Be("activeuser");
            result.Role.Should().Be("User");
            result.Token.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            _mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User[] { }); // 返回空數組，表示找不到用戶

            var loginDto = new LoginDto
            {
                Email = "notfound@example.com",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new[] { user });

            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "wrongpassword" // 錯誤密碼
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }
    }
}