#nullable enable
using PaymentGateway.BankSimulator.Model;

namespace PaymentGateway.BankSimulator.Interfaces;

public interface IBankClient
{
    public Task<string> GetSingleUseTokenAsync(
        CreditCardTokenizationRequest request, CancellationToken cancellationToken);
    
    public Task<CardAuthorizationResponse> ProcessPaymentByTokenAsync(
        string idempotencyKey, string token, string currency, long amount, CancellationToken cancellationToken);
    
    public Task<CardAuthorizationResponse?> GetPaymentAsync(
        string idempotencyKey, CancellationToken cancellationToken);
    
}