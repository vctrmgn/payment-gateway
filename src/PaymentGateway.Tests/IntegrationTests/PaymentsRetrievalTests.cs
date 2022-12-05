using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces.Services;
using PaymentGateway.SharedKernel.Enums;
using PaymentGateway.Web.AppStart;

namespace PaymentGateway.IntegratedTests.IntegrationTests;

public class PaymentsRetrievalTests
{
    [Fact]
    public async Task TestSinglePaymentRetrieval()
    {
        //Setup
        var scopedServiceProvider = TestingServiceProvider.BuildScope();

        var dbContext = scopedServiceProvider.GetDbContext();

        var testStore = dbContext.Merchants.Add(new Merchant { Name = "TestStore"}).Entity;
        
        await dbContext.SaveChangesAsync();

        var payment = new Payment()
        {
            SourceReference = Guid.NewGuid().ToString(),
            Amount = 2000,
            Currency = "GBP",
            CreationDate = new DateTime(2022, 10, 01),
            LastUpdate = new DateTime(2022, 10, 01),
            MaskedCardInfo = new MaskedCardInfo()
            {
                HolderName = "Tom Riddle",
                Number = "1111222233334444",
                ExpiryYear = 2023,
                ExpiryMonth = 12,
                Token = Guid.NewGuid().ToString()
            },
            MerchantId = testStore.Id,
            Merchant = testStore,
            BankOperationId = Guid.NewGuid().ToString(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            IdempotencyKeyExpiry = new DateTime(2022, 10, 02),
            Status = PaymentStatus.Authorized
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        //Run
        var paymentsRetrievalService = scopedServiceProvider.GetRequiredService<IPaymentRetrievalService>();

        var paymentDetails =  await paymentsRetrievalService
            .GetPaymentDetailsAsync(testStore.Id.ToString(), payment.Id.ToString(), CancellationToken.None);

        //Assert
        Assert.Equal(payment.SourceReference, paymentDetails.SourceReference);
        Assert.Equal(payment.Amount, paymentDetails.Amount);
        Assert.Equal(payment.Currency, paymentDetails.Currency);
        Assert.Equal(payment.CreationDate, paymentDetails.CreationDate);
        Assert.Equal(payment.LastUpdate, paymentDetails.LastUpdate);
        Assert.Equal(payment.Merchant.Name, paymentDetails.MerchantName);
        Assert.Equal(payment.MaskedCardInfo.HolderName, paymentDetails.CardHolderName);
        Assert.Equal(payment.MaskedCardInfo.Number, paymentDetails.CardNumber);
        Assert.Equal(payment.MaskedCardInfo.ExpiryYear, paymentDetails.CardExpiryYear);
        Assert.Equal(payment.MaskedCardInfo.ExpiryMonth, paymentDetails.CardExpiryMonth);
    }
    
    [Fact]
    public async Task TestMultiplePaymentsRetrieval()
    {
        //Setup
        var scopedServiceProvider = TestingServiceProvider.BuildScope();

        var dbContext = scopedServiceProvider.GetDbContext();

        var testStore = dbContext.Merchants.Add(new Merchant { Name = "TestStore"}).Entity;
        
        await dbContext.SaveChangesAsync();

        var totalPayments = 20;
        var filteringDate = new DateTime(2022, 12, 01);
        
        DatabaseInit.AddRandomPaymentsForExistingMerchants(dbContext, totalPayments, filteringDate);
        
        //Run
        var paymentsRetrievalService = scopedServiceProvider.GetRequiredService<IPaymentRetrievalService>();

        var filterCriteria = new PaymentsFilteringDto()
        {
            Limit = 10,
            Skip = 10,
            StartDate = filteringDate,
            EndDateInclusive = filteringDate,
        };
        
        var pagedResponse =  await paymentsRetrievalService
            .ListPaymentsDetailsAsync(testStore.Id.ToString(), filterCriteria, CancellationToken.None);

        //Assert
        Assert.Equal(filterCriteria.Limit, pagedResponse.Limit);
        Assert.Equal(filterCriteria.Skip, pagedResponse.Skip);
        Assert.Equal(totalPayments, pagedResponse.TotalItems);
        Assert.Equal(filterCriteria.Limit, pagedResponse.PageItems.Count);
    }
}