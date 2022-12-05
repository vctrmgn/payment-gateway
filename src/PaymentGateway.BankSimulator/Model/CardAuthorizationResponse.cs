#nullable disable
namespace PaymentGateway.BankSimulator.Model;

public class CardAuthorizationResponse
{
    public string OperationId { get; set; }
    public CardAuthorizationResponseStatus Status { get; set; }
}