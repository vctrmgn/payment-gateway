using PaymentGateway.Infrastructure.Security.Entities;

namespace PaymentGateway.Infrastructure.Security.Interfaces;

public interface IIdentityService
{
    Task<UserIdentity?> GetCredentialsProfile(string secret);
}