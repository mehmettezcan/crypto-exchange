using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace API.Middlewares;

public class ExceptionHandling(
    RequestDelegate next,
    ILogger<ExceptionHandling> logger,
    IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandling> _logger = logger;
    private readonly IHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            Console.WriteLine("Exception caught in middleware: " + ex);
            Console.WriteLine("Exception caught in middleware: " + ex.Message);

            var response = new ProblemDetails
            {
                Status = 500,
                Detail = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null,
                Title = ex.Message
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}