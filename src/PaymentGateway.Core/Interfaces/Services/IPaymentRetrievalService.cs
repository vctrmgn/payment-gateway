using PaymentGateway.Core.Dto;
using PaymentGateway.SharedKernel.Util;

namespace PaymentGateway.Core.Interfaces.Services;

public interface IPaymentRetrievalService
{
    Task<PaymentDetailsDto> GetPaymentDetailsAsync(
        string merchantId, 
        string paymentId, 
        CancellationToken cancellationToken);
    
    Task<PaginatedList<PaymentDetailsDto>> ListPaymentsDetailsAsync(
        string merchantId,
        PaymentsFilteringDto criteria, 
        CancellationToken cancellationToken);
}