using System.Net.Http.Json;
using System.Text.Json;
using BankMore.Transfer.Application.Interfaces;
using BankMore.Transfer.Infrastructure.Models;
using BankMore.Transfer.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankMore.Transfer.Infrastructure.Clients;

public class CurrentAccountApiClient : ICurrentAccountApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrentAccountApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CurrentAccountApiClient(HttpClient httpClient, IOptions<CurrentAccountApiOptions> options, ILogger<CurrentAccountApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var opts = options.Value;
        _httpClient.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Add("X-Internal-Api-Key", opts.InternalApiKey);
    }

    public async Task<string?> GetAccountIdByNumberAsync(int accountNumber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/internal/account-id?number={accountNumber}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<InternalAccountIdResponse>(JsonOptions, cancellationToken);
        return body?.AccountId;
    }

    public async Task DebitAsync(string accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var request = new InternalMovementRequest(AccountId: accountId, Amount: amount, Type: 'D');
        var response = await _httpClient.PostAsJsonAsync("api/internal/movement", request, JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Debit failed: {StatusCode} {Error}", response.StatusCode, error);
            throw new ApplicationException($"Current Account API returned {(int)response.StatusCode}: {error}");
        }
    }

    public async Task CreditAsync(string accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var request = new InternalMovementRequest(AccountId: accountId, Amount: amount, Type: 'C');
        var response = await _httpClient.PostAsJsonAsync("api/internal/movement", request, JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Credit failed: {StatusCode} {Error}", response.StatusCode, error);
            throw new ApplicationException($"Current Account API returned {(int)response.StatusCode}: {error}");
        }
    }
}
