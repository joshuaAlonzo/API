using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Api.DTOs;
using Api.UserModule;
using Api.Security;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IWebHostEnvironment _environment;

        private static string HashRawForSave(string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                throw new System.ArgumentException("Password is required.", nameof(rawPassword));
            }

            return PasswordHasher.Hash(rawPassword);
        }

        public AuthController(IUserRepository userRepository, IJwtTokenService jwtTokenService, IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _environment = environment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            if (dto.RoleId < 1)
            {
                return BadRequest("RoleId is required.");
            }

            var existingUser = await _userRepository.GetByUserNameAsync(dto.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            var user = new User
            {
                Username      = dto.Username,
                PasswordHash  = HashRawForSave(dto.Password),
                RoleId        = dto.RoleId,
                FirstName     = dto.FirstName,
                LastName      = dto.LastName,
                Email         = dto.Email,
                ContactNumber = dto.ContactNumber,
                CreatedAt     = DateTime.UtcNow,
            };

            await _userRepository.AddAsync(user);
            var createdUser = await _userRepository.GetByUserNameAsync(dto.Username);

            if (createdUser != null)
            {
                string token = _jwtTokenService.GenerateToken(createdUser);

                return Ok(new
                {
                    message = "User registered successfully.",
                    token,
                    userId = createdUser.UserId,
                    username = createdUser.Username,
                    roleId = createdUser.RoleId,
                });
            }

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _userRepository.GetByUserNameAsync(dto.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            bool isValid = PasswordHasher.Verify(user.PasswordHash, dto.Password);
            if (!isValid)
            {
                return Unauthorized("Invalid username or password.");
            }

            string token = _jwtTokenService.GenerateToken(user);
            return Ok(new
            {
                message = "Login successful.",
                token,
                userId = user.UserId,
                username = user.Username,
                roleId = user.RoleId,
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("Username is required.");
            }

            var user = await _userRepository.GetByUserNameAsync(dto.Username);
            if (user == null)
            {
                return Ok(new { message = "If the account exists, a reset token has been generated." });
            }

            string resetToken = _jwtTokenService.GeneratePasswordResetToken(user.UserId.ToString());

            // NOTE: exposed in response for local dev/demo only — a production API
            // would email this token instead of returning it directly, to avoid
            // letting anyone who knows a username steal that account's reset token.
            return Ok(new
            {
                message = "Reset token generated.",
                resetToken
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("Token and new password are required.");
            }

            if (!_jwtTokenService.TryValidatePasswordResetToken(dto.Token, out string userIdText))
            {
                return BadRequest("Invalid or expired reset token.");
            }

            if (!int.TryParse(userIdText, out var userId))
            {
                return BadRequest("Invalid reset token format.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid or expired reset token.");
            }

            user.PasswordHash = HashRawForSave(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}