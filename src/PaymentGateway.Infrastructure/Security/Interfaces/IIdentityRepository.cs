using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Security.Entities;

namespace PaymentGateway.Infrastructure.Security.Interfaces;

public interface IIdentityRepository : IBaseRepository<UserIdentity>
{
    Task<UserIdentity?> GetIdentity(string secret);
}