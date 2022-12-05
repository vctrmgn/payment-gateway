#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Infrastructure.DataAccess;
using PaymentGateway.Web.AppStart;

namespace PaymentGateway.IntegratedTests.IntegrationTests;

public class TestingServiceProvider
{
    private IServiceScope ServiceScope { get;  }

    private TestingServiceProvider(IServiceScope scope)
    {
        ServiceScope = scope;
    }
    
    public T GetRequiredService<T>()
    {
        return ServiceScope.ServiceProvider.GetRequiredService<T>();
    }

    public IConfiguration GetConfiguration()
    {
        return GetRequiredService<IConfiguration>();
    }

    public PaymentGatewayDbContext GetDbContext() 
    {
        return ServiceScope.ServiceProvider.GetRequiredService<PaymentGatewayDbContext>();
    }

    public PaymentGatewayDbContext GetSeedDbContext()
    {
        var dbContext = GetDbContext();
        
        DatabaseInit.SeedDatabase(dbContext, GetRequiredService<IConfiguration>());
        
        return dbContext;
    }
    
    public static TestingServiceProvider BuildScope()
    {
        var services = CreateServicesCollectionForTesting();

        services.AddDbContext<PaymentGatewayDbContext>(
            builder => builder.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddCoreServices();

        services.AddCoreAdapters();

        services.AddSecurityServices();

        services.AddExternalClients();

        services.AddAutoMappers();

        services.AddLogging();

        services.AddMemoryCache();

        return new TestingServiceProvider(services.BuildServiceProvider().CreateScope());
    }

    private static IServiceCollection CreateServicesCollectionForTesting() 
    {
        var services = new ServiceCollection();
        
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();
        services.AddSingleton(config);

        return services;
    }
}
//
// public class TestingContext
// {
//     public static TestingServiceProvider CreateServiceProvider()
//     {
//         var services = CreateServicesCollectionForTesting();
//
//         services.AddDbContext<PaymentGatewayDbContext>(
//             builder => builder.UseInMemoryDatabase(Guid.NewGuid().ToString()));
//
//         services.AddCoreServices();
//
//         services.AddCoreAdapters();
//
//         services.AddSecurityServices();
//
//         services.AddExternalClients();
//
//         services.AddAutoMappers();
//
//         return new TestingServiceProvider(services.BuildServiceProvider());
//     }
//
//     private static IServiceCollection CreateServicesCollectionForTesting() 
//     {
//         var services = new ServiceCollection();
//         
//         IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();
//         services.AddSingleton(config);
//
//         return services;
//     }
// }
