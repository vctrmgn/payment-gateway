using FluentValidation;
using PaymentGateway.Core.Dto;

namespace PaymentGateway.Core.Validators;

public class PaymentsFilteringValidator : AbstractValidator<PaymentsFilteringDto>
{
    private const int MaxSearchPeriodInDays = 31;
    
    public PaymentsFilteringValidator()
    {
        RuleFor(pf => pf.Limit).InclusiveBetween(10, 100);
        RuleFor(pf => pf.Skip).GreaterThanOrEqualTo(0);
        RuleFor(pf => pf.StartDate).GreaterThan(DateTime.MinValue);
        RuleFor(pf => pf.EndDateInclusive).GreaterThanOrEqualTo(pf => pf.StartDate);
        RuleFor(pf => (int)(pf.EndDateInclusive - pf.StartDate).TotalDays)
            .LessThanOrEqualTo(MaxSearchPeriodInDays)
            .WithMessage($"The search period should be less than {MaxSearchPeriodInDays} days.");
    }
}