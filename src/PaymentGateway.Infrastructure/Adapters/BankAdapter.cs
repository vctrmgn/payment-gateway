using System.ComponentModel;
using Microsoft.Extensions.Logging;
using PaymentGateway.BankSimulator.Interfaces;
using PaymentGateway.BankSimulator.Model;
using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces.Adapters;
using PaymentGateway.SharedKernel.Enums;
using PaymentGateway.SharedKernel.Exceptions;

namespace PaymentGateway.Infrastructure.Adapters;

public class BankAdapter : IBankAdapter
{
    private readonly IBankClient _bankClient;
    private readonly ILogger<BankAdapter> _logger;

    public BankAdapter(
        IBankClient bankClient,
        ILogger<BankAdapter> logger)
    {
        _bankClient = bankClient;
        _logger = logger;
    }

    public async Task<PaymentBankResponseDto> ProcessPaymentByTokenAsync(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            var bankResponse =
                await _bankClient.ProcessPaymentByTokenAsync(
                    payment.Id.ToString(), 
                    payment.MaskedCardInfo.Token, 
                    payment.Currency, 
                    payment.Amount,
                    cancellationToken);

            return BuildPaymentResponseDto(bankResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing payment on Bank: {ExMessage}", ex.Message);
            throw new PaymentProcessingOnBankException();
        }
    }

    public async Task<PaymentBankResponseDto?> GetPaymentAsync(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            var bankResponse = 
                await _bankClient.GetPaymentAsync(payment.Id.ToString(), cancellationToken);
        
            return bankResponse is null 
                ? null 
                : BuildPaymentResponseDto(bankResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting payment information from bank: {ExMessage}", ex.Message);
            throw new PaymentProcessingOnBankException();
        }
    }

    public async Task<string> GetSingleUseTokenAsync(CardInfoDto cardInfo, CancellationToken cancellationToken)
    {
        return await _bankClient.GetSingleUseTokenAsync(BuildTokenizationRequest(cardInfo), cancellationToken);
    }

    private static CreditCardTokenizationRequest BuildTokenizationRequest(CardInfoDto cardInfo)
    {
        return new CreditCardTokenizationRequest
        {
            CardNumber = cardInfo.Number,
            ExpiryYear = cardInfo.ExpiryYear,
            ExpiryMonth = cardInfo.ExpiryMonth,
            Cvv = cardInfo.Cvv,
        };
    }
    
    private static PaymentBankResponseDto BuildPaymentResponseDto(CardAuthorizationResponse bankResponse)
    {
        var paymentStatus = bankResponse.Status switch
        {
            CardAuthorizationResponseStatus.Authorized => PaymentStatus.Authorized,
            CardAuthorizationResponseStatus.Declined => PaymentStatus.Declined,
            CardAuthorizationResponseStatus.Verified => PaymentStatus.Verified,
            _ => throw new InvalidEnumArgumentException()
        };

        return new PaymentBankResponseDto()
        {
            BankOperationId = bankResponse.OperationId,
            PaymentStatus = paymentStatus
        };
    }
}