#nullable enable
namespace PaymentGateway.Core.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> Query();
    
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    
    Task<T?> FindAsync(T entity, CancellationToken cancellationToken);
}
