using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManagement.UserService.Data;
using EventManagement.UserService.Models;
using EventManagement.UserService.Models.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace EventManagement.UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> RegisterAsync(UserRegistrationDto userRegistration)
        {
            _logger.LogInformation("Checking if email exists: {Email}", userRegistration.Email);
            
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(userRegistration.Email))
            {
                _logger.LogWarning("Email already exists: {Email}", userRegistration.Email);
                return null;
            }

            _logger.LogInformation("Email is available, creating new user");
            
            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = userRegistration.Name,
                Email = userRegistration.Email,
                PasswordHash = _passwordHasher.HashPassword(userRegistration.Password),
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Saving new user to database: {UserId}", user.Id);
            await _userRepository.CreateUserAsync(user);

            // Generate JWT token
            _logger.LogInformation("Generating JWT token for user: {UserId}", user.Id);
            var token = GenerateJwtToken(user);

            _logger.LogInformation("User registration complete: {UserId}", user.Id);
            
            // Return authentication response
            return new AuthResponseDto
            {
                Token = token,
                User = MapUserToUserResponseDto(user),
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"))
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(UserLoginDto userLogin)
        {
            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(userLogin.Email);
            if (user == null)
            {
                return null;
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                return null;
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return authentication response
            return new AuthResponseDto
            {
                Token = token,
                User = MapUserToUserResponseDto(user),
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"))
            };
        }

        public async Task<UserResponseDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return MapUserToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> UpdateUserProfileAsync(Guid userId, UserUpdateDto userUpdate)
        {
            // Get existing user
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Check if email is being changed and already exists
            if (userUpdate.Email != user.Email && await _userRepository.EmailExistsAsync(userUpdate.Email))
            {
                return null;
            }

            // Update user properties
            user.Name = userUpdate.Name;
            user.Email = userUpdate.Email;
            user.UpdatedAt = DateTime.UtcNow;

            // Update password if provided
            if (!string.IsNullOrEmpty(userUpdate.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(userUpdate.Password);
            }

            // Save changes
            var updatedUser = await _userRepository.UpdateUserAsync(user);
            if (updatedUser == null)
            {
                return null;
            }

            return MapUserToUserResponseDto(updatedUser);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "YourSuperSecretKeyWithAtLeast32Characters"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public UserResponseDto MapUserToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
} 