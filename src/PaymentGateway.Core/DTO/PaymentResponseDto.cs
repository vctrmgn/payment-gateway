using PaymentGateway.SharedKernel.Enums;

namespace PaymentGateway.Core.Dto;

public record PaymentResponseDto
(
    string PaymentId,
    PaymentStatus PaymentStatus
);

    
