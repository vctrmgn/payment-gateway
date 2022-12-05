#nullable disable
using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public class Merchant : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
}