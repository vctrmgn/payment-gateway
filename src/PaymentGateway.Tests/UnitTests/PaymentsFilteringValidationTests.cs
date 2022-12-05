using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Validators;

namespace PaymentGateway.IntegratedTests.UnitTests;

public class PaymentsFilteringValidationTests
{
    private static readonly PaymentsFilteringDto ValidDto = 
        new()
        {
            Limit = 10,
            Skip = 0,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDateInclusive = DateTime.UtcNow
        };
    
    
    [Theory]
    [InlineData(10, true)]
    [InlineData(100, true)]
    [InlineData(9, false)]
    [InlineData(101, false)]
    public void TestLimitValues(int limit, bool shouldPass)
    {
        var passed = ValidateFilter(limit, ValidDto.Limit, ValidDto.StartDate, ValidDto.EndDateInclusive);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Theory]
    [InlineData(0, true)]
    [InlineData(int.MaxValue, true)]
    [InlineData(-1, false)]
    public void TestSkipValues(int skip, bool shouldPass)
    {
        var passed = ValidateFilter(ValidDto.Limit, skip, ValidDto.StartDate, ValidDto.EndDateInclusive);
        
        Assert.Equal(shouldPass, passed);
    }
    
    [Fact]
    public void TestValidDateRelationsShouldPass()
    {
        Assert.True(ValidateFilter(ValidDto.Limit, ValidDto.Skip, DateTime.UtcNow.AddDays(-31), DateTime.UtcNow));
        Assert.True(ValidateFilter(ValidDto.Limit, ValidDto.Skip, DateTime.UtcNow, DateTime.UtcNow));
        Assert.True(ValidateFilter(ValidDto.Limit, ValidDto.Skip, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow));
    }
    
    [Fact]
    public void TestInvalidDateRelationsShouldFails()
    {
        Assert.False(ValidateFilter(ValidDto.Limit, ValidDto.Skip, DateTime.UtcNow.AddDays(-32), DateTime.UtcNow));
        Assert.False(ValidateFilter(ValidDto.Limit, ValidDto.Skip, DateTime.UtcNow.AddDays(1), DateTime.UtcNow));
    }

    private static bool ValidateFilter(int limit, int skip, DateTime startDate, DateTime endDateInclusive)
    {
        var validator = new PaymentsFilteringValidator();
        
        var dto = new PaymentsFilteringDto()
        {
            Limit = limit,
            Skip = skip,
            StartDate = startDate,
            EndDateInclusive = endDateInclusive
        };

        var result = validator.Validate(dto);

        return result.IsValid;
    }
}