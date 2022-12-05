using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.DataAccess;
using PaymentGateway.SharedKernel.Enums;
using PaymentGateway.Web.Security;
using UserIdentity = PaymentGateway.Infrastructure.Security.Entities.UserIdentity;

namespace PaymentGateway.Web.AppStart;

public static class DatabaseInit
{
    public static void RunMigrations(IServiceScope serviceScope)
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<PaymentGatewayDbContext>();
        
#if DEBUG
        dbContext.Database.EnsureDeleted();
#endif
        dbContext.Database.Migrate();
        SeedDatabase(dbContext, serviceScope.ServiceProvider.GetRequiredService<IConfiguration>());
    }

    public static void SeedDatabase(PaymentGatewayDbContext dbContext, IConfiguration configuration)
    {
        AddMerchants(dbContext, configuration);
        AddRandomPaymentsForExistingMerchants(dbContext, 1000);
    }

    public static void AddMerchants(PaymentGatewayDbContext dbContext, IConfiguration configuration)
    {
        if(dbContext.Merchants.Any())
            return;
        
        var nerdStore = dbContext.Merchants.Add(new Merchant { Name = "NerdStore"}).Entity;
        
        var magnoStore = dbContext.Merchants.Add(new Merchant { Name = "MagnoStore" }).Entity;

        dbContext.SaveChanges();

        var salt = configuration["PasswordSalt"]; 
        
        dbContext.UserIdentities.Add(new UserIdentity()
        {
            User = nerdStore, 
            IsActive = true,
            Secret = BCrypt.Net.BCrypt.HashPassword("nerdstore_secret_example_1234", salt),
            RolesByComma = $"{Roles.PaymentsReader},{Roles.PaymentsRequester}"
        });
        
        dbContext.UserIdentities.Add(new UserIdentity()
        {
            User = nerdStore,  
            IsActive = true,
            Secret = BCrypt.Net.BCrypt.HashPassword("nerdstore_secret_example_5678", salt),
            RolesByComma = $"{Roles.PaymentsReader}"
        });
        
        dbContext.UserIdentities.Add(new UserIdentity()
        {
            User = magnoStore,  
            IsActive = true,
            Secret = BCrypt.Net.BCrypt.HashPassword("magnostore_secret_example_1234", salt),
            RolesByComma = $"{Roles.PaymentsReader},{Roles.PaymentsRequester}"
        });

        dbContext.SaveChanges();
    }

    public static void AddRandomPaymentsForExistingMerchants(
        PaymentGatewayDbContext dbContext, int totalPaymentsToAdd, DateTime? specificDate = null)
    {
        var merchants = dbContext.Merchants.ToList();
        
        var holderNames = new List<string> { "Fulano", "Ciclano", "Beltrano" };
        
        var currencies = new List<string> { "BRL", "GBP", "USD" };
        
        var cardNumbers = new List<string> { 
            "5186001700008785", 
            "5186001700009726", 
            "5186001700009908", 
            "5186001700008876", 
            "5186001700001434", 
            "4012888888881881", 
            "371449635398431",  
            "38520000023237"    
        };
        
        var random = new Random();
        
        for (var i = 0; i < totalPaymentsToAdd; i++)
        {
            var creationDate = DateTime.UtcNow.AddDays(-random.Next(0, 60));
            
            dbContext.Payments.Add(new Payment()
            {
                SourceReference = Guid.NewGuid().ToString(),
                Amount = random.Next(0, 100000_00),
                Currency = currencies[random.Next(0, currencies.Count)],
                CreationDate = specificDate ?? DateTime.UtcNow.AddDays(-random.Next(0, 60)),
                LastUpdate = DateTime.UtcNow,
                MaskedCardInfo = new MaskedCardInfo()
                {
                    HolderName = holderNames[random.Next(0, holderNames.Count)],
                    Number = cardNumbers[random.Next(0, cardNumbers.Count)],
                    ExpiryYear = 2023,
                    ExpiryMonth = 12,
                    Token = Guid.NewGuid().ToString()
                },
                MerchantId = merchants[random.Next(0, merchants.Count)].Id,
                BankOperationId = Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString(),
                IdempotencyKeyExpiry = creationDate.AddDays(1),
                Status = (PaymentStatus) random.Next(0, 4)
            });
        }
        
        dbContext.SaveChanges();
    }
}