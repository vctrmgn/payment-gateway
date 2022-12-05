#nullable enable
using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces.Adapters;

public interface IBankAdapter
{
    Task<PaymentBankResponseDto> ProcessPaymentByTokenAsync(Payment payment, CancellationToken cancellationToken);
    
    Task<PaymentBankResponseDto?> GetPaymentAsync(Payment payment, CancellationToken cancellationToken);
    
    Task<string> GetSingleUseTokenAsync(CardInfoDto cardInfo, CancellationToken cancellationToken);
}