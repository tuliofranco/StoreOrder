using Microsoft.EntityFrameworkCore;
using Order.Core.Application.Abstractions;
using Order.Core.Domain.Common;
using Order.Infrastructure.Persistence.Outbox;
using MediatR;

namespace Order.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly StoreOrderDbContext _db;
    private readonly IPublisher _publisher;

    public EfUnitOfWork(StoreOrderDbContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
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

        try
        {
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        var tasks = domainEvents
            .OfType<INotification>()
            .Select(e => _publisher.Publish(e, ct));

        await Task.WhenAll(tasks);
    }
}
