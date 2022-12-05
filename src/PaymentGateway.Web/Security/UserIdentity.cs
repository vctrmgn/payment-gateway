using System.Security.Claims;

namespace PaymentGateway.Web.Security;

public class UserIdentity : ClaimsIdentity 
{
    public string MerchantId { get; }
    public string MerchantName { get; }
    
    public UserIdentity(
        string authenticationType,
        string merchantId,
        string merchantName) : base(authenticationType)
    {
        MerchantId = merchantId;
        MerchantName = merchantName;
    }
}