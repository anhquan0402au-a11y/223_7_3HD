using Microsoft.Extensions.Primitives;

namespace OrderApi.Middleware
{

    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _Next;
        private readonly string _ExpectedKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _Next = next;

            string? key = configuration["ApiKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "dev-secret-key";
            }
            _ExpectedKey = key;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsGet(context.Request.Method))
            {
                await _Next(context);
                return;
            }

            if (context.Request.Headers.TryGetValue("X-Api-Key", out StringValues provided) == false)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API key.");
                return;
            }

            if (provided.ToString() != _ExpectedKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API key.");
                return;
            }

            await _Next(context);
        }
    }
}