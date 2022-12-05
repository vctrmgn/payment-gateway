using System.Text.RegularExpressions;
using FluentValidation;
using PaymentGateway.Core.Dto;

namespace PaymentGateway.Core.Validators;

public class PaymentRequestValidator : AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestValidator()
    {
        RuleFor(pr => pr.Amount).GreaterThanOrEqualTo(0);
        RuleFor(pr => pr.Currency).Must(cur => ValidCurrencies.Contains(cur)).WithMessage("Invalid Currency.");
        RuleFor(pr => pr.SourceReference).MaximumLength(50);
        
        RuleFor(pr => pr.CardInfo).NotNull();
        RuleFor(pr => pr.CardInfo.Number).NotEmpty().CreditCard();
        RuleFor(pr => pr.CardInfo.ExpiryYear).InclusiveBetween(2022, 2100);
        RuleFor(pr => pr.CardInfo.ExpiryMonth).InclusiveBetween(1, 12);
        
        RuleFor(pr => pr.CardInfo.HolderName)
            .NotEmpty()
            .MaximumLength(50)
            .Must(ValidatePersonName)
            .WithMessage("Invalid Card Holder Name.");
        
        RuleFor(pr => pr.CardInfo.Cvv)
            .MinimumLength(3)
            .MaximumLength(4)
            .Must(ValidateOnlyNumbers)
            .WithMessage("Invalid CVV.");
    }

    private static bool ValidateOnlyNumbers(string? value) =>
        value is not null && new Regex(@"^[0-9]*$").IsMatch(value);
    
    private static bool ValidatePersonName(string? name) =>
        name is not null && new Regex(@"^[a-zA-Z\s\u00C0-\u00FF]*$").IsMatch(name);

    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN", "BAM", "BBD", "BDT", "BGN", "BHD",
        "BIF", "BMD", "BND", "BOB", "BRL", "BSD", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHF", "CLF", "CLP",
        "CNY", "COP", "CRC", "CUP", "CVE", "CZK", "DJF", "DKK", "DOP", "DZD", "EEK", "EGP", "ERN", "ETB", "EUR",
        "FJD", "FKP", "GBP", "GEL", "GHS", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD", "HNL", "HRK", "HTG", "HUF",
        "IDR", "ILS", "INR", "IQD", "IRR", "ISK", "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "Kip", "KMF", "KRW",
        "KWD", "KYD", "KZT", "LAK", "Lek", "LKR", "LRD", "LSL", "LTL", "LVL", "LYD", "MAD", "MDL", "MGA", "MKD",
        "MMK", "MNT", "MOP", "MRO", "MUR", "MVR", "MWK", "MXN", "MYR", "MZN", "NAD", "NGN", "NIO", "NOK", "NPR",
        "NZD", "OMR", "PAB", "PEN", "PGK", "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD", "RWF", "SAR", "SBD",
        "SCR", "SDG", "SEK", "SGD", "SHP", "SLE", "Som", "SOS", "SRD", "STD", "SVC", "SYP", "SZL", "THB", "TJS",
        "TMT", "TND", "TOP", "TRY", "TTD", "TWD", "TZS", "UAH", "UGX", "USD", "UYU", "UZS", "VEF", "VND", "VUV",
        "WST", "XAF", "XCD", "XOF", "XPF", "Yen", "YER", "ZAR", "ZMW"
    };
}