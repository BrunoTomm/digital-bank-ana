using BankMore.Fees.Application.Interfaces;
using System.Collections.Concurrent;

namespace BankMore.Fees.Infrastructure.Idempotency;

public class InMemoryIdempotencyRepository : IIdempotencyRepository
{
    private readonly ConcurrentDictionary<string, byte> _processed = new();

    public Task<bool> ExistsAsync(string transferId, CancellationToken cancellationToken = default)
        => Task.FromResult(_processed.ContainsKey(transferId));

    public Task MarkProcessedAsync(string transferId, CancellationToken cancellationToken = default)
    {
        _processed.TryAdd(transferId, 0);
        return Task.CompletedTask;
    }
}
