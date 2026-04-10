using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _authService.RegisterAsync(registerDTO);

            if (!result.Success)
            {
                if (result.Message.Contains("error occurred"))
                {
                    return StatusCode(500, result.Message);
                }
                return BadRequest(result.Message);
            }

            return StatusCode(201, new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var token = await _authService.LoginAsync(loginDTO);
            if (token == null) return Unauthorized(new { message = "Invalid credentials" });

            // Create the cookie options
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Hides it from Angular/JavaScript
                Secure = true,   // Requires HTTPS
                SameSite = SameSiteMode.None, //Angular is port 4200, .NET is 7277
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            // Attach the cookie to the HTTP Response
            Response.Cookies.Append("jwt_token", token, cookieOptions);

            return Ok(new { message = "Logged in successfully" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("jwt_token", out var token) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "No active session found." });
            }

            // Hand off to the Service layer
            await _authService.LogoutAsync(token);

            Response.Cookies.Append("jwt_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            });

            return Ok(new { message = "Successfully logged out." });

        }

        [Authorize]
        [HttpDelete("delete-user-account")]
        public async Task<IActionResult> DeleteUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user ID in token.");
            }
            
            var deleted = await _authService.DeleteUserAsync(userId);
            if (!deleted)
            {
                return NotFound("User not found or already deleted.");
            }
            return Ok(new { message = "User deleted successfully." });
        }
        
        [Authorize]
        [HttpPost("save-vakifbank-consent")]
        public async Task<IActionResult> SaveVakifbankConsent([FromBody] string consentId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var saved = await _authService.SaveVakifbankConsentAsync(userId, consentId);
            if (!saved) return NotFound("User not found.");

            return Ok(new { message = "Vakifbank connected successfully!" });
        }

        [Authorize]
        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            return Ok(new { isAuthenticated = true });
        }
    }
}
