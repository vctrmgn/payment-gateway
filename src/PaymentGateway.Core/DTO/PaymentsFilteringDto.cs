#nullable disable
namespace PaymentGateway.Core.Dto;

public class PaymentsFilteringDto
{
    public int Limit { get; set; }
    public int Skip { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDateInclusive { get; set; }
    public string Reference { get; set; }
}