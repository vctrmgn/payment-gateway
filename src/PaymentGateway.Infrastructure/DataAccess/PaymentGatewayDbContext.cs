#nullable disable
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Security.Entities;

namespace PaymentGateway.Infrastructure.DataAccess;

public class PaymentGatewayDbContext : DbContext
{
    public DbSet<Payment> Payments { get; init; }
    
    public DbSet<MaskedCardInfo> CardInfos { get; init; }
    
    public DbSet<Merchant> Merchants { get; init; }
    
    public DbSet<UserIdentity> UserIdentities { get; init; }
    
    public PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
#if DEBUG
        options.LogTo(log => Debug.WriteLine(log));
#endif
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>().HasIndex(p => p.IdempotencyKey);
        
        modelBuilder.Entity<Payment>().HasIndex(p => p.IdempotencyKeyExpiry);
        
        modelBuilder.Entity<Payment>().HasIndex(p => p.SourceReference);
        
        modelBuilder.Entity<Payment>().HasIndex(p => p.CreationDate);

        modelBuilder.Entity<UserIdentity>().HasIndex(mi => mi.Secret).IsUnique();
    }
}