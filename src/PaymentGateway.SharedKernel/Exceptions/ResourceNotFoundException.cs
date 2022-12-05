namespace PaymentGateway.SharedKernel.Exceptions;

public class ResourceNotFoundException : Exception  
{
    public ResourceNotFoundException(string id) 
        : base($"The resource of Id '{id}' was not found.") 
    {
    }
}