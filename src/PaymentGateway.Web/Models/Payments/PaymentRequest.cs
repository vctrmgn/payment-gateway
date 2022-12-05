#nullable disable
using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Web.Models.Payments;

public class PaymentRequest
{
    public string SourceReference { get; set; }
    
    [Range(0, long.MaxValue)]
    public long? Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; }
    
    [Required]
    public CreditCard CardInfo { get; set; }
}
