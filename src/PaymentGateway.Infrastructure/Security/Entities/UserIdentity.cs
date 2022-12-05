#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Security.Entities;

public class UserIdentity : BaseEntity
{
    [Required]
    public string Secret { get; set; }
    
    [Required]
    public bool IsActive { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public Merchant User { get; set; }
    
    [Required]
    public string RolesByComma { get; set; }

    [NotMapped] 
    public IEnumerable<string> Roles => 
        string.IsNullOrEmpty(RolesByComma) 
            ? Array.Empty<string>()
            : RolesByComma.Split(",");
}