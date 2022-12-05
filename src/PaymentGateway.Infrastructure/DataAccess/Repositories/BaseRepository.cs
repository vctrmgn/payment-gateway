
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces.Repositories;

namespace PaymentGateway.Infrastructure.DataAccess.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    private readonly PaymentGatewayDbContext _dbContext;

    protected BaseRepository(PaymentGatewayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<T> Query()
    {
        return _dbContext.Set<T>().AsQueryable();
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        return (await _dbContext.AddAsync(entity, cancellationToken)).Entity;
    }

    public async Task<T?> FindAsync(T entity, CancellationToken cancellationToken)
    {
        return await _dbContext.FindAsync<T>(entity);
    }
}