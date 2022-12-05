#nullable enable
using PaymentGateway.BankSimulator.Interfaces;
using PaymentGateway.BankSimulator.Model;

namespace PaymentGateway.BankSimulator;

public class BankClient : IBankClient
{
    private readonly List<string> _alwaysAuthorizedCards = new()
    {
        "1111222233334444",
        "5186001700008785", 
        "5186001700009726", 
        "5186001700009908", 
        "5186001700008876", 
        "5186001700001434", 
        "4012888888881881", 
        "371449635398431",  
        "38520000023237"    
    };
    
    private readonly Dictionary<string, string> _cardNumbersByToken = new();
    
    private readonly Dictionary<string, CardAuthorizationResponse> _processedPayments = new();
    
    public Task<string> GetSingleUseTokenAsync(
        CreditCardTokenizationRequest request, 
        CancellationToken cancellationToken)
    {
        //Assuming Card Info is always valid

        var token = Guid.NewGuid().ToString();
        
        _cardNumbersByToken.Add(token, request.CardNumber);

        return Task.FromResult(token);
    }
    
    public Task<CardAuthorizationResponse> ProcessPaymentByTokenAsync(
        string idempotencyKey,
        string token,
        string currency,
        long amount,
        CancellationToken cancellationToken)
    {
        _cardNumbersByToken.TryGetValue(token, out var cardNumber);
        
        if(cardNumber is null)
            return Task.FromResult(BuildResponse(CardAuthorizationResponseStatus.Declined));
            
        var status =
            (_alwaysAuthorizedCards.Contains(cardNumber), amount) switch
            {
                ( true,  0 ) => CardAuthorizationResponseStatus.Verified,
                ( true, >0 ) => CardAuthorizationResponseStatus.Authorized,
                _ => CardAuthorizationResponseStatus.Declined,
            };

        var response = BuildResponse(status);
        
        _processedPayments.Add(idempotencyKey, response);

        _cardNumbersByToken.Remove(token);

        return Task.FromResult(response);
    }

    public Task<CardAuthorizationResponse?> GetPaymentAsync(
        string idempotencyKey, CancellationToken cancellationToken)
    {
        _processedPayments.TryGetValue(idempotencyKey, out var response);
        
        return Task.FromResult(response);
    }

    private static CardAuthorizationResponse BuildResponse(CardAuthorizationResponseStatus status)
    {
        return new CardAuthorizationResponse
        {
            OperationId = Guid.NewGuid().ToString(),
            Status = status
        };
    }
}