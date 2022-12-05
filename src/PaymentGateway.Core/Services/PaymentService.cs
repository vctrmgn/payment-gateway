using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces.Adapters;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Core.Interfaces.Services;
using PaymentGateway.Core.Validators;
using PaymentGateway.SharedKernel.Enums;
using PaymentGateway.SharedKernel.Exceptions;

namespace PaymentGateway.Core.Services;

public class PaymentService : BaseService, IPaymentService
{
    private readonly IBankAdapter _bankAdapter;
    private readonly IDistributedLockAdapter _lockAdapter;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly PaymentRequestValidator _paymentRequestValidator = new();

    private const int SingleTransactionLockInSeconds = 30;
    private const int SinglePaymentLockInHours = 24;
    
    private const string SingleTransactionGroupName = "transactions";
    private const string SinglePaymentGroupName = "payments";
    
    public PaymentService(
        IBankAdapter bankAdapter,
        IDistributedLockAdapter lockAdapter,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _bankAdapter = bankAdapter;
        _lockAdapter = lockAdapter;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(
        string merchantId,
        string idempotencyKey,
        PaymentRequestDto paymentRequestDto,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidatePaymentRequest(merchantId, paymentRequestDto);
            
            await TryLockForUniqueProcessing(idempotencyKey, cancellationToken);

            var isNewPayment = await LockForSinglePaymentAsync(idempotencyKey, cancellationToken);
            
            return await (isNewPayment 
                ? ProcessNewPaymentAsync(merchantId, idempotencyKey, paymentRequestDto, cancellationToken)
                : TryReprocessingPaymentAsync(merchantId, idempotencyKey, cancellationToken));
        }
        finally
        {
            await ReleaseLockForUniqueProcessing(idempotencyKey, cancellationToken);
        }
    }
    
    public async Task<PaymentResponseDto> ReprocessPaymentAsync(
        string merchantId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidateEntityIds(merchantId);
            
            await TryLockForUniqueProcessing(idempotencyKey, cancellationToken);
            
            return await TryReprocessingPaymentAsync(merchantId, idempotencyKey, cancellationToken);
        }
        finally
        {
            await ReleaseLockForUniqueProcessing(idempotencyKey, cancellationToken);
        }
    }
    
    private async Task<PaymentResponseDto> ProcessNewPaymentAsync(
        string merchantId,
        string idempotencyKey, 
        PaymentRequestDto paymentRequestDto,
        CancellationToken cancellationToken)
    {
        Payment? payment = null;
        
        try
        {
            payment = await BuildNewTokenizedPayment(merchantId, idempotencyKey, paymentRequestDto, cancellationToken);
            
            await _paymentRepository.AddAsync(payment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var responseDto = await _bankAdapter.ProcessPaymentByTokenAsync(payment, cancellationToken);

            payment.SetBankOperationResult(responseDto.BankOperationId, responseDto.PaymentStatus);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PaymentResponseDto(payment.Id.ToString(), payment.Status);
        }
        catch (Exception)
        {
            if (payment is not null && payment.Id != Guid.Empty)
            {
                return new PaymentResponseDto(payment.Id.ToString(), PaymentStatus.Unknown);
            }
            
            await ReleaseLockForSinglePaymentAsync(idempotencyKey, cancellationToken);
            throw new PaymentProcessingException(); 
        }
    }

    private async Task<PaymentResponseDto> TryReprocessingPaymentAsync(
        string merchantId,
        string idempotencyKey, 
        CancellationToken cancellationToken)
    {
        Payment? payment = null;
        
        try
        {
            payment = await _paymentRepository
                .GetPaymentByIdempotencyKeyAsync(Guid.Parse(merchantId), idempotencyKey, cancellationToken);

            if (payment is null)
            {
                await ReleaseLockForSinglePaymentAsync(idempotencyKey, cancellationToken);
                throw new InvalidLockStateException(); 
            }

            if (payment.Status is not PaymentStatus.Unknown)
                return BuildPaymentResponse(payment);;

            var paymentResponse = 
                await _bankAdapter.GetPaymentAsync(payment, cancellationToken) ??
                await _bankAdapter.ProcessPaymentByTokenAsync(payment, cancellationToken);

            payment.SetBankOperationResult(paymentResponse.BankOperationId, paymentResponse.PaymentStatus);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BuildPaymentResponse(payment);
        }
        catch (Exception)
        {
            if (payment is not null && payment.Id != Guid.Empty)
            {
                return new PaymentResponseDto(payment.Id.ToString(), PaymentStatus.Unknown);
            }
            
            throw new PaymentProcessingException();
        }
    }

    private void ValidatePaymentRequest(string merchantId, PaymentRequestDto paymentRequestDto)
    {
        ValidateEntityIds(merchantId);

        var validationResult = _paymentRequestValidator.Validate(paymentRequestDto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ToString());
        }
    }

    private static PaymentResponseDto BuildPaymentResponse(Payment payment) =>
        new (payment.Id.ToString(), payment.Status);
    
    private async Task<Payment> BuildNewTokenizedPayment(
        string merchantId, 
        string idempotencyKey, 
        PaymentRequestDto paymentRequestDto, 
        CancellationToken cancellationToken)
    {
        var token = await _bankAdapter.GetSingleUseTokenAsync(paymentRequestDto.CardInfo, cancellationToken);

        var maskedCardInfo = new MaskedCardInfo()
        {
            HolderName = paymentRequestDto.CardInfo.HolderName, 
            Number = paymentRequestDto.CardInfo.Number,
            ExpiryYear = paymentRequestDto.CardInfo.ExpiryYear,
            ExpiryMonth = paymentRequestDto.CardInfo.ExpiryMonth,
            Token = token
        };
        
        return new Payment
        {
            Amount = paymentRequestDto.Amount,
            Currency = paymentRequestDto.Currency,
            SourceReference = paymentRequestDto.SourceReference,
            CreationDate = DateTime.UtcNow,
            LastUpdate = DateTime.UtcNow,
            MerchantId = Guid.Parse(merchantId),
            Status = PaymentStatus.Unknown,
            MaskedCardInfo = maskedCardInfo,
            IdempotencyKey = idempotencyKey,
            IdempotencyKeyExpiry = DateTime.UtcNow.AddHours(SinglePaymentLockInHours), 
        };
    }

    private async Task TryLockForUniqueProcessing(string idempotencyKey, CancellationToken cancellationToken)
    {
        var isLockAcquired = await _lockAdapter
            .LockAsync($"{SingleTransactionGroupName}/{idempotencyKey}", 
                TimeSpan.FromSeconds(SingleTransactionLockInSeconds), cancellationToken);

        if (!isLockAcquired)
            throw new DuplicateProcessingException(idempotencyKey);
    }

    private async Task ReleaseLockForUniqueProcessing(string idempotencyKey,
        CancellationToken cancellationToken)
    {
        await _lockAdapter.ReleaseAsync($"{SingleTransactionGroupName}/{idempotencyKey}", cancellationToken);
    }
    
    private async Task<bool> LockForSinglePaymentAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var isLockAcquired = await _lockAdapter
            .LockAsync($"{SinglePaymentGroupName}/{idempotencyKey}", 
                TimeSpan.FromHours(SinglePaymentLockInHours), cancellationToken);

        return isLockAcquired;
    }
    
    private async Task ReleaseLockForSinglePaymentAsync(string idempotencyKey,
        CancellationToken cancellationToken)
    {
        await _lockAdapter.ReleaseAsync($"{SinglePaymentGroupName}/{idempotencyKey}", cancellationToken);
    }
}
