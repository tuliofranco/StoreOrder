using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Order.Core.Application.Abstractions;

namespace Order.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly StoreOrderDbContext _db;

    public EfUnitOfWork(StoreOrderDbContext db)
    {
        _db = db;
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }
}