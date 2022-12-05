namespace PaymentGateway.Core.Interfaces.Adapters;

public interface IDistributedLockAdapter : IDisposable
{
    Task<bool> LockAsync(string key, TimeSpan ttl, CancellationToken cancellationToken);
    Task ReleaseAsync(string key, CancellationToken cancellationToken);
}