using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PaymentGateway.Infrastructure.Security.Interfaces;

namespace PaymentGateway.Web.Security;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SCHEME = "ApiKey";
    public const string NAME = "API Key Authentication Handler";

    private const string AuthorizationType = "bearer";

    private readonly IIdentityService _identityService;

    public ApiKeyAuthenticationHandler(
        IIdentityService identityService,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _identityService = identityService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var secret = GetSecret(Context.Request.Headers);

            if (string.IsNullOrEmpty(secret))
                return AuthenticateResult.Fail("InvalidToken");

            var profile = await _identityService.GetCredentialsProfile(secret);

            if (profile is null)
                return AuthenticateResult.Fail("NotFound");
            
            Logger.LogInformation("Access from '{Id}' authorized", profile.User.Id);
            return AuthenticateResult.Success(GetAuthenticationTicketWithClaims(profile));
        }
        catch (Exception ex)
        {
            Logger.LogError("{ExMessage}", ex.Message);
            return AuthenticateResult.Fail("NotFound");
        }
    }

    private static string GetSecret(IHeaderDictionary headerDictionary)
    {
        var authorizationHeader = headerDictionary.Authorization.ToString();
        
        return 
            authorizationHeader.ToLower().StartsWith(AuthorizationType) 
                ? authorizationHeader[AuthorizationType.Length..].Trim() 
                : string.Empty;  
    }

    private static AuthenticationTicket GetAuthenticationTicketWithClaims(Infrastructure.Security.Entities.UserIdentity profile)
    {
        var identity = new UserIdentity(SCHEME, profile.UserId.ToString(), profile.User.Name);
        
        foreach (var role in profile.Roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        return new AuthenticationTicket(new ClaimsPrincipal(identity), SCHEME);
    }
}
