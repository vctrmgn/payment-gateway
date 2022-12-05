using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces.Repositories;

namespace PaymentGateway.Infrastructure.DataAccess.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(PaymentGatewayDbContext dbContext)
        : base(dbContext)
    {
    }
    
    public async Task<Payment?> GetPaymentAsync(Guid merchantId, Guid paymentId, CancellationToken cancellationToken)
    {
        return await 
            GetPaymentAsync(payment => 
                payment.MerchantId == merchantId && payment.Id == paymentId, cancellationToken);
    }

    public async Task<Payment?> GetPaymentByIdempotencyKeyAsync(Guid merchantId, string idempotencyKey, CancellationToken cancellationToken)
    {
        return await 
            GetPaymentAsync(payment => 
                payment.MerchantId == merchantId && 
                payment.IdempotencyKey == idempotencyKey &&
                payment.IdempotencyKeyExpiry >= DateTime.UtcNow, cancellationToken);
    }

    private Task<Payment?> GetPaymentAsync(
        Expression<Func<Payment, bool>> criteria, CancellationToken cancellationToken)
    {
        return Query()
            .AsNoTracking()
            .Include(p => p.MaskedCardInfo)
            .Include(p => p.Merchant)
            .FirstOrDefaultAsync(criteria, cancellationToken);
    }
    
    public async Task<(IEnumerable<Payment> pageItems, int totalItems)> GetPaymentsAsync(
        Guid merchantId, 
        PaymentsFilteringDto filteringDtoInfo,
        CancellationToken cancellationToken)
    {
        var queryable = Query()
            .AsNoTracking()
            .Include(p => p.MaskedCardInfo)
            .Include(p => p.Merchant)
            .Where(payment =>
                payment.MerchantId == merchantId &&
                payment.CreationDate >= filteringDtoInfo.StartDate &&
                payment.CreationDate <= filteringDtoInfo.EndDateInclusive &&
                (
                    string.IsNullOrEmpty(filteringDtoInfo.Reference) ||
                    payment.SourceReference == filteringDtoInfo.Reference
                ));

        var payments = 
            await queryable
                .OrderByDescending(payment => payment.CreationDate)
                .Skip(filteringDtoInfo.Skip)
                .Take(filteringDtoInfo.Limit)
                .ToListAsync(cancellationToken);
        
        var totalItems = 
            await queryable.CountAsync(cancellationToken);
        
        return (payments, totalItems);
    }
}