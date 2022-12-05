using PaymentGateway.Core.Interfaces.Repositories;

namespace PaymentGateway.Infrastructure.DataAccess;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly PaymentGatewayDbContext _dbContext;
    
    private bool _disposed;
    
    public UnitOfWork(PaymentGatewayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public void Dispose()
    {
        LocalDispose();
        GC.SuppressFinalize(this);
    }
    
    private void LocalDispose()
    {
        if (_disposed) return;
        
        _dbContext.Dispose();
        _disposed = true;
    }
}