using PaymentGateway.Core.Dto;

namespace PaymentGateway.Core.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(
        string merchantId,
        string idempotencyKey,
        PaymentRequestDto paymentRequestDto, 
        CancellationToken cancellationToken);

    //To be used by a worker in order to reprocess Pending/Unknown Payments
    //For errors occurred during communication with the Bank
    Task<PaymentResponseDto> ReprocessPaymentAsync(
        string merchantId,
        string paymentId,
        CancellationToken cancellationToken);
}