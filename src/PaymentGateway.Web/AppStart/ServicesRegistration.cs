using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PaymentGateway.BankSimulator;
using PaymentGateway.BankSimulator.Interfaces;
using PaymentGateway.Core.Dto.Mapping;
using PaymentGateway.Core.Interfaces.Adapters;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Core.Interfaces.Services;
using PaymentGateway.Core.Services;
using PaymentGateway.Infrastructure.Adapters;
using PaymentGateway.Infrastructure.DataAccess;
using PaymentGateway.Infrastructure.DataAccess.Repositories;
using PaymentGateway.Infrastructure.Security.Interfaces;
using PaymentGateway.Infrastructure.Security.Services;
using PaymentGateway.Web.Models.Mapping;
using PaymentGateway.Web.Security;

namespace PaymentGateway.Web.AppStart;

public static class RegisterServices
{
    public static IServiceCollection AddPaymentGatewayDbContext(this IServiceCollection services)
    {
        var migrationsAssembly = typeof(PaymentGatewayDbContext).GetTypeInfo().Assembly.GetName().Name;
        
        services.AddDbContext<PaymentGatewayDbContext>(
            builder => builder.UseSqlite("Data Source=payment-gateway.db", 
                options => options.MigrationsAssembly(migrationsAssembly)));

        return services;
    }
    
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentRetrievalService, PaymentRetrievalService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
    
    public static IServiceCollection AddCoreAdapters(this IServiceCollection services)
    {
        services.AddSingleton<IBankAdapter, BankAdapter>();
        services.AddSingleton<IDistributedLockAdapter, DistributedLockAdapter>();
        
        return services;
    }
    
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        
        return services;
    }
    
    public static IServiceCollection AddExternalClients(this IServiceCollection services)
    {
        services.AddSingleton<IBankClient, BankClient>();
        
        return services;
    }
    
    public static IServiceCollection AddApiKeyAuthenticationHandler(this IServiceCollection services)
    {
        services
            .AddAuthentication(options => options.DefaultScheme = ApiKeyAuthenticationHandler.SCHEME)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>
                (ApiKeyAuthenticationHandler.SCHEME, ApiKeyAuthenticationHandler.NAME, null);        

        return services;
    }
    
    public static IServiceCollection AddAuthorizationWithApiKeyPolicyAsDefault(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            var defaultPolicy = 
                new AuthorizationPolicyBuilder(ApiKeyAuthenticationHandler.SCHEME)
                    .RequireAuthenticatedUser()
                    .Build();
    
            options.AddPolicy("default", defaultPolicy);
        });     

        return services;
    }

    public static IServiceCollection AddAutoMappers(this IServiceCollection services)
    {
        services.AddAutoMapper(new [] { typeof(WebMapping), typeof(EntityToDtoMapper) });
        return services;
    }

    public static IServiceCollection AddSwaggerWithAuthentication(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Payment Gateway API",
                Description = "Payment Gateway API",
                Contact = new OpenApiContact
                {
                    Name = "Victor Magno",
                    Url = new Uri("https://www.linkedin.com/in/victormagno/")
                }
            });
    
            options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter the Bearer Authorization string as following: `Bearer Merchant-API-Secret`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
    
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
             
        });     

        return services;
    }
}