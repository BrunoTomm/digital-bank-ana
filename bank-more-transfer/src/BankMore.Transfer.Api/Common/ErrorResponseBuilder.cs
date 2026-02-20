using System.Net;
using System.Text.Json;
using BankMore.Transfer.Application.Common;

namespace BankMore.Transfer.Api.Common;

public static class ErrorResponseBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static (HttpStatusCode StatusCode, FailureType FailureType, string Message) MapForResponse(Exception ex)
        => ExceptionToFailureMapper.Map(ex);

    public static async Task WriteAsync(HttpContext context, HttpStatusCode statusCode, FailureType failureType, string message, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { message, failureType = failureType.ToCode() }, JsonOptions);
        await context.Response.WriteAsync(body, cancellationToken);
    }

    public static async Task WriteFromExceptionAsync(HttpContext context, Exception ex, CancellationToken cancellationToken = default)
    {
        var (statusCode, failureType, message) = MapForResponse(ex);
        await WriteAsync(context, statusCode, failureType, message, cancellationToken);
    }
}
