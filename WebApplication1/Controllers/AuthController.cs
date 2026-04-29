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
            var result = await _authService.LoginAsync(loginDTO);
            if (result == null) return Unauthorized(new { message = "Invalid credentials" });

            var (accessToken, refreshToken) = result.Value;

            Response.Cookies.Append("jwt_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            // Store refresh token in a separate HttpOnly cookie
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/Auth" // Scoped: only sent to auth endpoints
            });

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

            Response.Cookies.Append("refresh_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/api/Auth"
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


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken)
                || string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No refresh token found." });

            var result = await _authService.RefreshAsync(refreshToken);

            if (result == null)
                return Unauthorized(new { message = "Refresh token is invalid or expired. Please log in again." });

            var (newAccessToken, newRefreshToken) = result.Value;

            Response.Cookies.Append("jwt_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/Auth"
            });

            return Ok(new { message = "Token refreshed." });
        }
    }
}
