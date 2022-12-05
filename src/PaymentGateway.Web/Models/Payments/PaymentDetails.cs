#nullable disable
namespace PaymentGateway.Web.Models.Payments;

public class PaymentDetails
{
    public string PaymentId { get; set; }
    public string SourceReference { get; set; }
    public long Amount { get; set; }
    public string Currency { get; set; }
    public string CardHolderName { get; set; }
    public string CardNumber { get; set; }
    public int CardExpiryYear { get; set; }
    public int CardExpiryMonth { get; set; }
    public string MerchantName { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
}