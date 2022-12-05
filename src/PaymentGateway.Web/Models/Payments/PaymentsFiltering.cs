using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Web.Models.Payments;

public class PaymentsFiltering
{
    [FromQuery]
    [Range(10, 100)]
    [DefaultValue(10)]
    public int Limit { get; set; } 
    
    [FromQuery]
    [Range(0, int.MaxValue)]
    [DefaultValue(0)]
    public int Skip { get; set; } 
    
    [FromQuery]
    [StringLength(maximumLength: 50)]
    public string? Reference { get; set; }
    
    [FromQuery]
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    
    [FromQuery]
    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDateInclusive { get; set; }
}