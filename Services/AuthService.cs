using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceAdminAPI.DTOs;
using EcommerceAdminAPI.Models;
using EcommerceAdminAPI.Repositories;
using BCrypt.Net;

namespace EcommerceAdminAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IGenericRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await GetUserByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            var token = GenerateJwtToken(user);
            var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationInHours");

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
            };
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return null;
            }

            var existingUsername = (await _userRepository.FindAsync(u => u.Username == registerDto.Username)).FirstOrDefault();
            if (existingUsername != null)
            {
                return null;
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            var token = GenerateJwtToken(user);
            var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationInHours");

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
            };
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await _userRepository.FindAsync(u => u.Email == email);
            return users.FirstOrDefault();
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(jwtSettings.GetValue<int>("ExpirationInHours")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}