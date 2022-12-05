using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces.Services;
using PaymentGateway.SharedKernel.Enums;

namespace PaymentGateway.IntegratedTests.IntegrationTests;

public class PaymentsCreationTests
{
    [Fact]
    public async Task TestPaymentCreation()
    {
        //Setup Payment Request
        var scopedServiceProvider = TestingServiceProvider.BuildScope();

        var dbContext = scopedServiceProvider.GetDbContext();

        var testStore = dbContext.Merchants.Add(new Merchant { Name = "TestStore"}).Entity;
        
        await dbContext.SaveChangesAsync();

        var paymentRequest = new PaymentRequestDto()
        {
            SourceReference = Guid.NewGuid().ToString(),
            Amount = 2000,
            Currency = "GBP",
            CardInfo = new CardInfoDto()
            {
                HolderName = "Tom Riddle",
                Number = "1111222233334444",
                ExpiryYear = 2023,
                ExpiryMonth = 12,
                Cvv = "0000"
            }
        };

        //Run Payment Processing
        var paymentsService = scopedServiceProvider.GetRequiredService<IPaymentService>();
        var paymentsRetrievalService = scopedServiceProvider.GetRequiredService<IPaymentRetrievalService>();

        var paymentResponse = await paymentsService.ProcessPaymentAsync(
            testStore.Id.ToString(), Guid.NewGuid().ToString(), paymentRequest,CancellationToken.None);
        
        //Assert Results
        Assert.NotEqual(Guid.Empty.ToString(), paymentResponse.PaymentId);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse.PaymentStatus);
        
        //Run Payment Query
        var paymentDetails = await paymentsRetrievalService.GetPaymentDetailsAsync(
            testStore.Id.ToString(), paymentResponse.PaymentId,CancellationToken.None);
        
        //Assert Results
        Assert.Equal(paymentRequest.SourceReference, paymentDetails.SourceReference);
        Assert.Equal(paymentRequest.Amount, paymentDetails.Amount);
        Assert.Equal(paymentRequest.Currency, paymentDetails.Currency);
        Assert.Equal(DateTime.UtcNow.Date, paymentDetails.CreationDate.Date);
        Assert.Equal(DateTime.UtcNow.Date, paymentDetails.LastUpdate.Date);
        Assert.Equal("T********e", paymentDetails.CardHolderName);
        Assert.Equal("1111********4444", paymentDetails.CardNumber);
        Assert.Equal(paymentRequest.CardInfo.ExpiryYear, paymentDetails.CardExpiryYear);
        Assert.Equal(paymentRequest.CardInfo.ExpiryMonth, paymentDetails.CardExpiryMonth);
    }
}