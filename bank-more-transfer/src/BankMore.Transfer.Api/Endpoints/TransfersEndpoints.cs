using BankMore.Transfer.Api.Common;
using BankMore.Transfer.Api.Models;
using BankMore.Transfer.Application.Commands.Transfer;
using BankMore.Transfer.Application.Common;
using MediatR;
using System.Security.Claims;

namespace BankMore.Transfer.Api.Endpoints;

public static class TransfersEndpoints
{
    public static IEndpointRouteBuilder MapTransfersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transfers").WithTags("Transfers");

        group.MapPost("/", async (TransferRequest request, HttpContext context, IMediator mediator, CancellationToken ct) =>
        {
            var accountId = context.User.FindFirstValue("accountId");
            if (string.IsNullOrEmpty(accountId)) return Results.Forbid();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            try
            {
                var result = await mediator.Send(new TransferCommand(accountId, request.DestinationAccountNumber, request.Amount, correlationId), ct);
                return Results.Ok(new TransferResponse(result.TransferId));
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .RequireAuthorization()
        .WithName("Transfer")
        .WithSummary("Realizar transferência")
        .WithDescription("Transfere valor da conta autenticada para outra conta (por número). Gera tarifa automática via Kafka.")
        .Produces<TransferResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);

        return routes;
    }
}
