namespace WebApplication1.Services
{
    public interface IGeminiIntegrationService
    {
        Task<string> GetChatResponseAsync(string userPrompt, string? customContext = null);
    }
}