namespace PaymentGateway.SharedKernel.Exceptions;

public class DuplicateProcessingException : Exception
{
    public DuplicateProcessingException(string idempotencyKey) 
        : base($"Duplicate processing attempt identified. Idempotency Key: '{idempotencyKey}'.") 
    {
    }
}