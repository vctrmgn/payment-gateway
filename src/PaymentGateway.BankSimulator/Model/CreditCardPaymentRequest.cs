#nullable disable
namespace PaymentGateway.BankSimulator.Model;

public class CreditCardPaymentRequest
{
    public long? Amount { get; set; }

    public string Currency { get; set; }
    
    public string CardNumber { get; set; }
    
    public int ExpiryYear { get; set; }

    public int ExpiryMonth { get; set; }
    
    public string Cvv { get; set; }
}