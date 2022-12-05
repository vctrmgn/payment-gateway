using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<Payment?> GetPaymentAsync(
        Guid merchantId, Guid paymentId, CancellationToken cancellationToken);
    
    Task<Payment?> GetPaymentByIdempotencyKeyAsync(
        Guid merchantId, string idempotencyKey, CancellationToken cancellationToken);
    
    Task<(IEnumerable<Payment> pageItems, int totalItems)> GetPaymentsAsync(
        Guid merchantId, 
        PaymentsFilteringDto filteringDtoInfo, 
        CancellationToken cancellationToken);
}