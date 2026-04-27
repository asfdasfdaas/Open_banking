using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace WebApplication1.Filters
{

    [AttributeUsage(AttributeTargets.Method)]
    public class IdempotentAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Look for the Idempotency Key in the HTTP Headers
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
            {
                // If it's missing, reject the request. The Controller never runs.
                context.Result = new BadRequestObjectResult(new { message = "Idempotency Key is missing from headers." });
                return;
            }

            // Grab the cache service from the application
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

            // Check if we have already processed this exact key
            if (cache.TryGetValue(idempotencyKey, out string? cachedResponse))
            {
                // We already did this transfer. 
                // Return a fake success message without hitting the database.
                context.Result = new OkObjectResult(new { message = "Transfer completed successfully!" });
                return;
            }

            // We haven't seen this key yet. Allow the Controller method to run!
            var executedContext = await next();

            // The Controller finished. Did it succeed?
            if (executedContext.Exception == null && executedContext.Result is OkObjectResult or OkResult)
            {
                // If it succeeded, save the key to the cache so we remember it
                cache.Set(idempotencyKey, "Processed", TimeSpan.FromHours(1));
            }
        }
    }
}