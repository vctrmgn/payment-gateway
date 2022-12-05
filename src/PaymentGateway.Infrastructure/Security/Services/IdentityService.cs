using Microsoft.Extensions.Configuration;
using PaymentGateway.Infrastructure.Security.Entities;
using PaymentGateway.Infrastructure.Security.Interfaces;

namespace PaymentGateway.Infrastructure.Security.Services;

public class IdentityService : IIdentityService
{
    private readonly IIdentityRepository _identityRepository;
    private readonly string _salt;

    public IdentityService(IConfiguration configuration, IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
        _salt = configuration["PasswordSalt"];
    }

    public Task<UserIdentity?> GetCredentialsProfile(string secret)
    {
        var hashedSecret = BCrypt.Net.BCrypt.HashPassword(secret, _salt);

        return _identityRepository.GetIdentity(hashedSecret);
    }
}
