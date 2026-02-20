using BankMore.CurrentAccount.Api.Common;
using BankMore.CurrentAccount.Api.Models;
using BankMore.CurrentAccount.Application.Commands.InactivateAccount;
using BankMore.CurrentAccount.Application.Commands.Login;
using BankMore.CurrentAccount.Application.Commands.Movement;
using BankMore.CurrentAccount.Application.Commands.RegisterCurrentAccount;
using BankMore.CurrentAccount.Application.Common;
using BankMore.CurrentAccount.Application.Queries.Balance;
using MediatR;
using System.Security.Claims;

namespace BankMore.CurrentAccount.Api.Endpoints;

public static class AccountsEndpoints
{
    public static IEndpointRouteBuilder MapAccountsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapPost("/register", async (RegisterRequest request, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var result = await mediator.Send(new RegisterCurrentAccountCommand(request.Cpf, request.Password), ct);
                return Results.Ok(new RegisterResponse(result.AccountNumber));
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithSummary("Registrar nova conta")
        .WithDescription("Cria uma nova conta corrente com CPF e senha. Retorna o número da conta.")
        .Produces<RegisterResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", async (LoginRequest request, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var result = await mediator.Send(new LoginCommand(request.AccountNumberOrCpf, request.Password), ct);
                return Results.Ok(new LoginResponse(result.Token, result.AccountId, result.AccountNumber));
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Autenticar conta")
        .WithDescription("Autentica por número de conta ou CPF e senha. Retorna JWT para uso nos demais endpoints.")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/inactivate", async (InactivateRequest request, HttpContext context, IMediator mediator, CancellationToken ct) =>
        {
            var accountId = context.User.FindFirstValue("accountId");
            if (string.IsNullOrEmpty(accountId)) return Results.Forbid();
            try
            {
                await mediator.Send(new InactivateAccountCommand(accountId, request.Password), ct);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .RequireAuthorization()
        .WithName("Inactivate")
        .WithSummary("Inativar conta")
        .WithDescription("Inativa a conta autenticada. Requer confirmação da senha.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/movement", async (MovementRequest request, HttpContext context, IMediator mediator, CancellationToken ct) =>
        {
            var accountId = context.User.FindFirstValue("accountId");
            if (string.IsNullOrEmpty(accountId)) return Results.Forbid();
            try
            {
                await mediator.Send(new MovementCommand(request.AccountNumber?.ToString(), accountId, request.Value, request.Type), ct);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .RequireAuthorization()
        .WithName("AccountsMovement")
        .WithSummary("Crédito ou débito")
        .WithDescription("Registra movimentação (C=crédito, D=débito) na conta do token ou em conta específica por número.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/balance", async (HttpContext context, IMediator mediator, CancellationToken ct) =>
        {
            var accountId = context.User.FindFirstValue("accountId");
            if (string.IsNullOrEmpty(accountId)) return Results.Forbid();
            try
            {
                var result = await mediator.Send(new BalanceQuery(accountId), ct);
                return Results.Ok(new BalanceResponse(result.AccountNumber, result.HolderName, result.QueriedAt, result.Balance));
            }
            catch (Exception ex)
            {
                var (statusCode, failureType, message) = ErrorResponseBuilder.MapForResponse(ex);
                return Results.Json(new { message, failureType = failureType.ToCode() }, statusCode: (int)statusCode);
            }
        })
        .RequireAuthorization()
        .WithName("GetBalance")
        .WithSummary("Consultar saldo")
        .WithDescription("Retorna o saldo atual da conta autenticada.")
        .Produces<BalanceResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);

        return routes;
    }
}
