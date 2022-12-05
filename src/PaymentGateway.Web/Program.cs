#nullable disable
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;
using PaymentGateway.Web.AppStart;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPaymentGatewayDbContext();

builder.Services.AddCoreServices();

builder.Services.AddCoreAdapters();

builder.Services.AddSecurityServices();

builder.Services.AddExternalClients();

builder.Services.AddAutoMappers();

builder.Services.AddApiKeyAuthenticationHandler();

builder.Services.AddAuthorizationWithApiKeyPolicyAsDefault();

builder.Services.AddSwaggerWithAuthentication();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();



var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture)
});

app.UseSwagger();

app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"); });

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

DatabaseInit.RunMigrations(app.Services.CreateScope());

app.Run();