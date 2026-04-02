using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IAiAssistantService _aiAssistantService;

        public AiController(IAiAssistantService aiAssistantService)
        {
            _aiAssistantService = aiAssistantService;
        }

        // create a tiny DTO just for the Angular request
        public class ChatRequestDto { public string Prompt { get; set; } = string.Empty; }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { message = "Prompt cannot be empty" });

            try
            {
                var responseText = await _aiAssistantService.ChatAsync(request.Prompt);

                return Ok(new { reply = responseText });// anonymous object 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("analyze-spending/{accountNumber}")]
        public async Task<IActionResult> AnalyzeSpending(string accountNumber, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out var parsedStartDate) || !DateTime.TryParse(endDate, out var parsedEndDate))
                    return BadRequest(new { message = "Invalid date format." });

                var result = await _aiAssistantService.AnalyzeSpendingAsync(accountNumber, parsedStartDate, parsedEndDate);

                if (!result.AccountFound)
                    return NotFound(new { message = "Account not found." });

                return Ok(new { advice = result.Advice });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}