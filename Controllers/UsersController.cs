using System.Security.Claims;
using EventManagement.UserService.Models.DTOs;
using EventManagement.UserService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAuthService authService, ILogger<UsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] UserRegistrationDto registrationDto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registrationDto.Email);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for registration: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Model validation passed, attempting to register user");
            var result = await _authService.RegisterAsync(registrationDto);
            if (result == null)
            {
                _logger.LogWarning("Registration failed for email: {Email}, email already in use", registrationDto.Email);
                return BadRequest(new { message = "Email already in use" });
            }

            _logger.LogInformation("User registered successfully: {Email}", registrationDto.Email);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<ActionResult<UserResponseDto>> UpdateCurrentUser([FromBody] UserUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized();
            }

            var user = await _authService.UpdateUserProfileAsync(id, updateDto);
            if (user == null)
            {
                return BadRequest(new { message = "User update failed. Email may already be in use." });
            }

            return Ok(user);
        }
    }
} 