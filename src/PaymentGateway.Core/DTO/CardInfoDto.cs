#nullable disable
namespace PaymentGateway.Core.Dto;

public class CardInfoDto 
{
    public string Number  { get; set; } 
    
    public string HolderName { get; set; }
    
    public int ExpiryYear { get; set; }
    
    public int ExpiryMonth { get; set; }
    
    public string Cvv { get; set; }
}
