#nullable disable
using System.ComponentModel.DataAnnotations;
using PaymentGateway.SharedKernel.Extensions;

namespace PaymentGateway.Core.Entities;

public class MaskedCardInfo : BaseEntity
{
    private string _number;
    [Required]
    public string Number
    {
        get => _number;
        set => _number = value.Mask(4, 4);
    }
    
    private string _holderName;
    [Required]
    public string HolderName 
    {
        get => _holderName;
        set => _holderName = value.Mask(1, 1);
    }

    [Required]
    public int ExpiryYear { get; set; }

    [Required]
    public int ExpiryMonth { get; set; }
    
    [Required]
    public string Token  { get; set; }
}
