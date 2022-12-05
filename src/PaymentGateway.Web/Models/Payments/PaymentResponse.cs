using PaymentGateway.SharedKernel.Enums;

namespace PaymentGateway.Web.Models.Payments;

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}