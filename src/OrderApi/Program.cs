using OrderApi.Middleware;
using OrderApi.Services;
using Prometheus;

namespace OrderApi
{

    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IOrderService, OrderService>();
            builder.Services.AddHealthChecks();

            WebApplication app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            // Collect HTTP metrics (request count, duration, in-progress) for Prometheus.
            app.UseHttpMetrics();

            // Protect write endpoints with an API key.
            app.UseMiddleware<ApiKeyMiddleware>();

            app.MapControllers();

            // Expose /metrics for Prometheus and a simple /health endpoint.
            app.MapMetrics();
            app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

            app.MapHealthChecks("/health");

            
            app.Run();
        }
    }
}