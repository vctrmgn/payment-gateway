#nullable disable
namespace PaymentGateway.BankSimulator.Model;

public class CreditCardTokenizationRequest
{
    public string CardNumber { get; set; }
    
    public int ExpiryYear { get; set; }

    public int ExpiryMonth { get; set; }
    
    public string Cvv { get; set; }
}