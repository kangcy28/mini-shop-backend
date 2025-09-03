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
    /// <summary>
    /// Service for handling user authentication operations including login, registration, and JWT token generation
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the AuthService
        /// </summary>
        /// <param name="userRepository">Repository for user data operations</param>
        /// <param name="configuration">Application configuration settings</param>
        public AuthService(IGenericRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="loginDto">Login credentials containing email and password</param>
        /// <returns>Authentication response with JWT token if successful, null if authentication fails</returns>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await GetUserByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            // Check if user account is active
            if (!user.IsActive)
            {
                return null; // Account is inactive
            }

            // Generate JWT token for authenticated user
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

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="registerDto">Registration details including username, email, password, and role</param>
        /// <returns>Authentication response with JWT token if registration successful, null if user already exists</returns>
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email is already registered
            var existingUser = await GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return null; // Email already exists
            }

            // Check if username is already taken
            var existingUsername = (await _userRepository.FindAsync(u => u.Username == registerDto.Username)).FirstOrDefault();
            if (existingUsername != null)
            {
                return null; // Username already exists
            }

            // Create new user with hashed password
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save user to database
            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            // Generate JWT token for newly registered user
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

        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns>User object if found, null otherwise</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await _userRepository.FindAsync(u => u.Email == email);
            return users.FirstOrDefault();
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user
        /// </summary>
        /// <param name="user">The user to create the token for</param>
        /// <returns>JWT token as string</returns>
        public string GenerateJwtToken(User user)
        {
            // Get JWT configuration settings
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            // Create user claims for the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Configure token properties
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

            // Create and return the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}