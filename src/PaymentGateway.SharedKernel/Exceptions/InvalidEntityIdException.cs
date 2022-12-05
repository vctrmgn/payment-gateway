namespace PaymentGateway.SharedKernel.Exceptions;

public class InvalidEntityIdException : Exception
{
    public InvalidEntityIdException() 
        : base("One or more invalid identifiers were informed.")
    {
    }
}