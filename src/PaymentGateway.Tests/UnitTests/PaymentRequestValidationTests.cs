using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Validators;

namespace PaymentGateway.IntegratedTests.UnitTests;

public class PaymentRequestValidationTests
{
    private static readonly CardInfoDto ValidCardDto =
        new()
        {
            Number = "5186001700008785",
            ExpiryYear = 2023,
            ExpiryMonth = 12,
            Cvv = "828",
            HolderName = "Minerva McGonagall"
        };
        
    
    private static readonly PaymentRequestDto ValidPaymentDto = 
        new()
        {
            Amount = 1,
            Currency = "GBP",
            SourceReference = Guid.NewGuid().ToString(),
            CardInfo = ValidCardDto
        };
    
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(long.MaxValue, true)]
    [InlineData(-1, false)]
    public void TestAmountValues(long amount, bool shouldPass)
    {
        var passed = ValidatePaymentRequest(
            amount, ValidPaymentDto.Currency, ValidPaymentDto.SourceReference, ValidPaymentDto.CardInfo);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData("brl", true)]
    [InlineData("gbp", true)]
    [InlineData("usd", true)]
    [InlineData("BRL", true)]
    [InlineData("GBP", true)]
    [InlineData("USD", true)]
    [InlineData("US", false)]
    [InlineData("real", false)]
    public void TestCurrencyValues(string currency, bool shouldPass)
    {
        var passed = ValidatePaymentRequest(
            ValidPaymentDto.Amount, currency, ValidPaymentDto.SourceReference, ValidPaymentDto.CardInfo);
        
        Assert.Equal(shouldPass, passed);
    } 
    
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("order-123", true)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipisci...", false)] //Too Long
    public void TestReferenceValues(string reference, bool shouldPass)
    {
        var passed = ValidatePaymentRequest(ValidPaymentDto.Amount, ValidPaymentDto.Currency, reference, ValidPaymentDto.CardInfo);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData("38520000023237", true)]   
    [InlineData("371449635398431", true)]  
    [InlineData("5186001700008785", true)] 
    [InlineData("5186001700009726", true)] 
    [InlineData("5186001700009908", true)] 
    [InlineData("5186001700008876", true)] 
    [InlineData("5186001700001434", true)] 
    [InlineData("4012888888881881", true)] 
    [InlineData("1111", false)]
    [InlineData("1111222233334444", true)]
    [InlineData("AAAAAAAAAAAAAAAA", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("1111111111111111111111111", false)]
    public void TestCardNumberValues(string cardNumber, bool shouldPass)
    {
        var passed = ValidateCardInfo(
            cardNumber, ValidCardDto.HolderName, ValidCardDto.ExpiryYear, ValidCardDto.ExpiryMonth, ValidCardDto.Cvv);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData("Victor Magno", true)]
    [InlineData("Fulano de Tal", true)]
    [InlineData("John Doe", true)]
    [InlineData("Günter", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("Harry 123", false)]
    [InlineData("Luke!", false)]
    [InlineData("Pedro de Alcântara Francisco Antônio João Carlos Xavier de Paula Miguel", false)]
    public void TestHolderNameValues(string holderName, bool shouldPass)
    {
        var passed = ValidateCardInfo(
            ValidCardDto.Number, holderName, ValidCardDto.ExpiryYear, ValidCardDto.ExpiryMonth, ValidCardDto.Cvv);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData(2022, true)]
    [InlineData(2100, true)]
    [InlineData(2050, true)]
    [InlineData(2021, false)]
    [InlineData(2101, false)]
    public void TestExpiryYearValues(int expiryYear, bool shouldPass)
    {
        var passed = ValidateCardInfo(
            ValidCardDto.Number, ValidCardDto.HolderName, expiryYear, ValidCardDto.ExpiryMonth, ValidCardDto.Cvv);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData(1, true)]
    [InlineData(12, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(13, false)]
    public void TestExpiryMonthValues(int expiryMonth, bool shouldPass)
    {
        var passed = ValidateCardInfo(
            ValidCardDto.Number, ValidCardDto.HolderName, ValidCardDto.ExpiryYear, expiryMonth, ValidCardDto.Cvv);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData("000", true)]
    [InlineData("999", true)]
    [InlineData("0000", true)]
    [InlineData("9999", true)]
    [InlineData("aaa", false)]
    [InlineData("aaaa", false)]
    [InlineData("-000", false)]
    public void TestCvvValues(string cvv, bool shouldPass)
    {
        var passed = ValidateCardInfo(
            ValidCardDto.Number, ValidCardDto.HolderName, ValidCardDto.ExpiryYear, ValidCardDto.ExpiryMonth, cvv);
        
        Assert.Equal(shouldPass, passed);
    }

    private static bool ValidateCardInfo(
        string number, string holderName, int expiryYear, int expiryMonth, string cvv)
    {
        var cardDto = new CardInfoDto
        {
            Number = number,
            HolderName = holderName,
            ExpiryYear = expiryYear,
            ExpiryMonth = expiryMonth,
            Cvv = cvv
        };
        
        return ValidatePaymentRequest(
            ValidPaymentDto.Amount, ValidPaymentDto.Currency, ValidPaymentDto.SourceReference, cardDto);
    }

    private static bool ValidatePaymentRequest(long amount, string currency, string sourceReference, CardInfoDto cardInfoDto)
    {
        var validator = new PaymentRequestValidator();
        
        var dto = new PaymentRequestDto()
        {
            Amount = amount,
            Currency = currency,
            SourceReference = sourceReference,
            CardInfo = cardInfoDto
        };

        var result = validator.Validate(dto);

        return result.IsValid;
    }
}