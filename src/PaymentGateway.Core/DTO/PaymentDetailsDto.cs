#nullable disable
using PaymentGateway.SharedKernel.Enums;

namespace PaymentGateway.Core.Dto;

public class PaymentDetailsDto
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
    public PaymentStatus Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
}