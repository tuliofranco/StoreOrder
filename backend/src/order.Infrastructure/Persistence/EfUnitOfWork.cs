using Microsoft.EntityFrameworkCore;
using Order.Core.Application.Abstractions;
using Order.Core.Domain.Common;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly StoreOrderDbContext _db;

    public EfUnitOfWork(StoreOrderDbContext db)
    {
        _db = db;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var aggregates = _db.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IAggregateRoot>()
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = OutboxMessageFactory.FromDomainEvent(domainEvent);
            await _db.OutboxMessages.AddAsync(outboxMessage, ct);
        }

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
