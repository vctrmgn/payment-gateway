#nullable enable
namespace PaymentGateway.SharedKernel.Extensions;

public static class StringExtensions
{
    public static string Mask(this string? str, int initialBlockSize, int finalBlockSize)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        
        var maskedBlockSize = str.Length - initialBlockSize - finalBlockSize;

        return maskedBlockSize > 0 
            ? $"{str[..initialBlockSize]}{new string('*', maskedBlockSize)}{str[^finalBlockSize..]}"
            : new string('*', str.Length);
    }
}