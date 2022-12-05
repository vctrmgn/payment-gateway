using PaymentGateway.Core.Entities;
using PaymentGateway.SharedKernel.Extensions;

namespace PaymentGateway.IntegratedTests.UnitTests;

public class MaskingTests
{
    [Fact]
    public void TestMasking()
    {
        const string cardNumber = "4444555566667777";
        const string emptyString = "";
        string? nullString = null;

        var validMaskedCardNumber = cardNumber.Mask(4, 4);
        var invalidMaskedCardNumber = cardNumber.Mask(10, 10);
        var maskedEmptyString = emptyString.Mask(6, 4);
        var maskedNullString = nullString.Mask(6, 4);
        
        Assert.Equal("4444********7777", validMaskedCardNumber);
        Assert.Equal(new string('*', cardNumber.Length), invalidMaskedCardNumber);
        Assert.Equal("", maskedEmptyString);
        Assert.Equal("", maskedNullString);
    }
    
    [Fact]
    public void TestCreditCardMasking()
    {
        var creditCard = new MaskedCardInfo()
        {
            Number = "38520000023237",
            HolderName = "John Doe",
            ExpiryYear = 2022,
            ExpiryMonth = 12
        };
        
        Assert.True(creditCard.Number.Contains('*'));
        Assert.True(creditCard.HolderName.Contains('*'));
        Assert.Equal(2022, creditCard.ExpiryYear);
        Assert.Equal(12, creditCard.ExpiryMonth);
    }
}