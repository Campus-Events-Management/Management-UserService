using EventManagement.UserService.Models;
using EventManagement.UserService.Models.DTOs;

namespace EventManagement.UserService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(UserRegistrationDto userRegistration);
        Task<AuthResponseDto?> LoginAsync(UserLoginDto userLogin);
        Task<UserResponseDto?> GetUserProfileAsync(Guid userId);
        Task<UserResponseDto?> UpdateUserProfileAsync(Guid userId, UserUpdateDto userUpdate);
        string GenerateJwtToken(User user);
        UserResponseDto MapUserToUserResponseDto(User user);
    }
} 