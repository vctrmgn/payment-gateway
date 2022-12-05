#nullable disable
using PaymentGateway.SharedKernel.Enums;

namespace PaymentGateway.Core.Dto;

public class PaymentBankResponseDto
{
    public string BankOperationId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}
