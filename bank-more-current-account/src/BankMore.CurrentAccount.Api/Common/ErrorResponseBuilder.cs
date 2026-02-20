using System.Net;
using System.Text.Json;
using BankMore.CurrentAccount.Application.Common;

namespace BankMore.CurrentAccount.Api.Common;

public static class ErrorResponseBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task WriteAsync(HttpContext context, HttpStatusCode statusCode, FailureType failureType, string message, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { message, failureType = failureType.ToCode() }, JsonOptions);
        await context.Response.WriteAsync(body, cancellationToken);
    }

    public static (HttpStatusCode StatusCode, FailureType FailureType, string Message) MapForResponse(Exception ex)
    {
        var (statusCode, failureType, message) = ExceptionToFailureMapper.Map(ex);
        if (ex is UnauthorizedAccessException && message == "Invalid credentials.")
            statusCode = HttpStatusCode.Forbidden;
        return (statusCode, failureType, message);
    }

    public static async Task WriteFromExceptionAsync(HttpContext context, Exception ex, CancellationToken cancellationToken = default, string? detail = null)
    {
        var (statusCode, failureType, message) = MapForResponse(ex);
        var finalMessage = string.IsNullOrEmpty(detail) ? message : $"{message} ({detail})";
        await WriteAsync(context, statusCode, failureType, finalMessage, cancellationToken);
    }
}
