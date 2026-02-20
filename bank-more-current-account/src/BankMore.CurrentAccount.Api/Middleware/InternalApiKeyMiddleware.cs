using BankMore.CurrentAccount.Application.Common;

namespace BankMore.CurrentAccount.Api.Middleware;

public class InternalApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Internal-Api-Key";

    public InternalApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Path.StartsWithSegments("/api/internal", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }
        var expectedKey = configuration["Internal:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey))
        {
            await _next(context);
            return;
        }
        var providedKey = context.Request.Headers[HeaderName].FirstOrDefault();
        if (providedKey != expectedKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or missing internal API key.", failureType = FailureType.UserUnauthorized.ToCode() });
            return;
        }
        await _next(context);
    }
}
