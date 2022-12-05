#nullable disable
using System.ComponentModel.DataAnnotations;
using PaymentGateway.SharedKernel.Enums;

namespace PaymentGateway.Core.Entities;

public class Payment : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string IdempotencyKey { get; set; }  
    
    [Required]
    public DateTime IdempotencyKeyExpiry { get; set; } 
    
    [MaxLength(50)]
    public string SourceReference { get; set; } 
    
    [Required]
    public long Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; }
    
    [Required]
    public DateTime CreationDate { get; set; } 
    
    [Required]
    public DateTime LastUpdate { get; set; }
    
    [Required]
    public MaskedCardInfo MaskedCardInfo { get; set; }
    
    [Required]
    public Guid MerchantId { get; set; } 
    
    public Merchant Merchant { get; set; }
    
    [Required]
    public PaymentStatus Status { get; set; }
    
    [MaxLength(50)]
    public string BankOperationId { get; set; }

    public void SetBankOperationResult(string operationId, PaymentStatus status)
    {
        BankOperationId = operationId;
        Status = status;
    }
}

