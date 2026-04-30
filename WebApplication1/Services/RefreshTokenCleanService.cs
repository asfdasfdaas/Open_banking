using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public class RefreshTokenCleanService : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanService> _logger;

        public RefreshTokenCleanService(
            IServiceScopeFactory scopeFactory,
            ILogger<RefreshTokenCleanService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                    var now = DateTime.UtcNow;

                    var deletedCount = await dbContext.RefreshTokens
                        .Where(rt => rt.IsRevoked || rt.ExpiresAt < now)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (deletedCount > 0)
                    {
                        _logger.LogInformation("RefreshTokenCleanService deleted {DeletedCount} revoked refresh tokens.", deletedCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RefreshTokenCleanService encountered an error during cleanup.");
                }

                await Task.Delay(CleanupInterval, stoppingToken);
            }
        }
    }
}
