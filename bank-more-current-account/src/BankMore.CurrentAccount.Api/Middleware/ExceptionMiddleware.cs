using BankMore.CurrentAccount.Api.Common;
using BankMore.CurrentAccount.Application.Common;

namespace BankMore.CurrentAccount.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        _logger.LogError(ex, "Unhandled error. CorrelationId: {CorrelationId}", correlationId);

        var detail = _env.IsDevelopment() ? (ex.InnerException?.Message ?? ex.Message) : null;
        await ErrorResponseBuilder.WriteFromExceptionAsync(context, ex, context.RequestAborted, detail);
    }
}
