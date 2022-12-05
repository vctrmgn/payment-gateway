#nullable disable
namespace PaymentGateway.Core.Dto;

public class PaymentRequestDto
{
    public string SourceReference { get; set; }
    
    public long Amount { get; set; }

    private string _currency;
    public string Currency
    {
        get => _currency;
        set => _currency = value?.ToUpper();
    }
    
    public CardInfoDto CardInfo { get; set; }
}