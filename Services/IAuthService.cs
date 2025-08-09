using EcommerceAdminAPI.DTOs;
using EcommerceAdminAPI.Models;

namespace EcommerceAdminAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<User?> GetUserByEmailAsync(string email);
        string GenerateJwtToken(User user);
    }
}