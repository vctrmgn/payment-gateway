using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Security.Entities;
using PaymentGateway.Infrastructure.Security.Interfaces;

namespace PaymentGateway.Infrastructure.DataAccess.Repositories;

public class IdentityRepository : BaseRepository<UserIdentity>, IIdentityRepository
{
    public IdentityRepository(PaymentGatewayDbContext dbContext) : base(dbContext)
    {
    }

    public Task<UserIdentity?> GetIdentity(string secret)
    {
        return Query()
            .Include(i => i.User)
            .SingleOrDefaultAsync(c => c.IsActive && c.Secret.Equals(secret));
    }
}