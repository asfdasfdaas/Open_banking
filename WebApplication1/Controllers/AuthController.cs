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
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (await _repo.UserExists(registerDTO.Username.ToLower()))
            {
                return BadRequest("Username already exists.");
            }
            if (await _repo.EmailExists(registerDTO.Email.ToLower()))
            {
                return BadRequest("Email already exists.");
            }
            var userToCreate = new User
            {
                UserName = registerDTO.Username.ToLower(),
                Email = registerDTO.Email.ToLower()

            };
            var createdUser = await _repo.Register(userToCreate, registerDTO.Password);
            if (createdUser == null)
            {
                return StatusCode(500, "An error occurred while creating the user.");
            }
            return StatusCode(201, new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var token = await _repo.Login(loginDTO.Username.ToLower(), loginDTO.Password);

            if (token == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { token = token, expires = DateTime.Now.AddDays(1) });
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
            var deleted = await _repo.DeleteUser(userId);
            if (!deleted)
            {
                return NotFound("User not found or already deleted.");
            }
            return Ok(new { message = "User deleted successfully." });  
        }
    }
}
