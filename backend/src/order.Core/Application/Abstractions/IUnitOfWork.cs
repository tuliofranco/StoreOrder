
namespace Order.Core.Application.Abstractions;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct = default);
}