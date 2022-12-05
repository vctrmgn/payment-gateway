#nullable disable
using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Web.Models.Payments;

public class CreditCard
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string HolderName { get; set; }
    
    [Required]
    [CreditCard]
    public string Number { get; set; }
    
    [Required]
    [Range(2022, 2100)] 
    public int ExpiryYear { get; set; }

    [Required]
    [Range(1, 12)]
    public int ExpiryMonth { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 3)]
    public string Cvv { get; set; }
}
