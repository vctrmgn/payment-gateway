using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using PaymentGateway.Core.Interfaces.Adapters;

namespace PaymentGateway.Infrastructure.Adapters;

public class DistributedLockAdapter : IDistributedLockAdapter
{
    private readonly ILogger<DistributedLockAdapter> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly HashSet<string> _lockedKeys;

    public DistributedLockAdapter(IMemoryCache memoryCache, ILogger<DistributedLockAdapter> logger)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _lockedKeys = new HashSet<string>();
    }
    
    public Task<bool> LockAsync(string key, TimeSpan ttl, CancellationToken _)
    {
        var isLockAcquired = _lockedKeys.Add(key);
        
        if (!isLockAcquired) return Task.FromResult(false);

        _memoryCache.Set(key, string.Empty, BuildCacheEntryOptions(ttl));
        
        return Task.FromResult(true);
    }

    private MemoryCacheEntryOptions BuildCacheEntryOptions(TimeSpan ttl)
    {
        return new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ttl)
            .AddExpirationToken(GenerateExpirationToken(ttl))
            .RegisterPostEvictionCallback((expiredObj, _, _, _) =>
            {
                var expiredKey = (string)expiredObj;
                _lockedKeys.Remove(expiredKey);
                _logger.LogInformation("Lock expired for Key: {ExpiredKey}", expiredKey);
            });  
    }

    private static CancellationChangeToken GenerateExpirationToken(TimeSpan ttl)
    {
        return new CancellationChangeToken(new CancellationTokenSource(ttl.Add(TimeSpan.FromSeconds(1))).Token);
    }

    public Task ReleaseAsync(string key, CancellationToken cancellationToken)
    {
        _lockedKeys.Remove(key);
        _memoryCache.Remove(key);
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}