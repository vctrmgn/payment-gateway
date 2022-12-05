using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentGateway.Web.Security;

public sealed class FromIdentity : ModelBinderAttribute
{
    public FromIdentity() : base(typeof(IdentityBinder))
    {
        BindingSource = BindingSource.Special;
    }
}

internal class IdentityBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        bindingContext.Result = 
            ModelBindingResult.Success(bindingContext.HttpContext.User.Identity as UserIdentity);
        
        return Task.CompletedTask;
    }
}
