using BankMore.CurrentAccount.Api.Common;
using BankMore.CurrentAccount.Api.Models;
using BankMore.CurrentAccount.Application.Commands.Movement;
using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Application.Common;
using MediatR;

namespace BankMore.CurrentAccount.Api.Endpoints;

public static class InternalEndpoints
{
    public static IEndpointRouteBuilder MapInternalEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/internal").WithTags("Internal");

        group.MapGet("/account-id", async (int number, ICurrentAccountRepository accountRepository, CancellationToken ct) =>
        {
            var account = await accountRepository.GetByNumberAsync(number, ct);
            if (account == null) return Results.NotFound();
            return Results.Ok(new InternalAccountIdResponse(account.Id));
        })
        .WithName("GetAccountId")
        .WithSummary("Obter ID da conta por número")
        .WithDescription("Retorna o ID (UUID) da conta dado o número. Uso interno (Transfer). Requer X-Internal-Api-Key.")
        .Produces<InternalAccountIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/movement", async (InternalMovementRequest request, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                await mediator.Send(new MovementCommand(null, request.AccountId, request.Amount, request.Type), ct);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .WithName("InternalMovement")
        .WithSummary("Movimentação interna")
        .WithDescription("Débito ou crédito em conta por ID. Uso interno (Transfer). Requer X-Internal-Api-Key.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        return routes;
    }
}
