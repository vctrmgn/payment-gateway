using PaymentGateway.SharedKernel.Exceptions;

namespace PaymentGateway.Core.Services;

public class BaseService
{
    protected static void ValidateEntityIds(params string[] ids)
    {
        foreach(var id in ids)
        {
            Guid.TryParse(id, out Guid result);

            if (result == Guid.Empty)
                throw new InvalidEntityIdException();
        }
    }
}