using AutoMapper;
using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Core.Interfaces.Services;
using PaymentGateway.Core.Validators;
using PaymentGateway.SharedKernel.Exceptions;
using PaymentGateway.SharedKernel.Util;

namespace PaymentGateway.Core.Services;

public class PaymentRetrievalService : BaseService, IPaymentRetrievalService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    private readonly PaymentsFilteringValidator _paymentsFilteringValidator = new();

    public PaymentRetrievalService(
        IPaymentRepository paymentRepository, 
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }
    
    public async Task<PaymentDetailsDto> GetPaymentDetailsAsync(
        string merchantId, 
        string paymentId, 
        CancellationToken cancellationToken)
    {
        ValidateEntityIds(merchantId, paymentId);
        
        var payment = 
            await _paymentRepository.GetPaymentAsync(
                Guid.Parse(merchantId), Guid.Parse(paymentId), cancellationToken);

        return _mapper.Map<PaymentDetailsDto>(payment);
    }

    public async Task<PaginatedList<PaymentDetailsDto>> ListPaymentsDetailsAsync(
        string merchantId,
        PaymentsFilteringDto criteria,
        CancellationToken cancellationToken)
    {
        ValidatePaymentsListingRequest(merchantId, criteria);

        var (pageItems, totalItems) = await _paymentRepository.GetPaymentsAsync(
            Guid.Parse(merchantId),
            criteria,
            cancellationToken);
        
        return new PaginatedList<PaymentDetailsDto>(
            pageItems.Select(p => _mapper.Map<PaymentDetailsDto>(p)).ToList(), 
            criteria.Limit, 
            criteria.Skip, 
            totalItems);
    }

    private void ValidatePaymentsListingRequest(string merchantId, PaymentsFilteringDto criteria)
    {
        ValidateEntityIds(merchantId);

        var validationResult = _paymentsFilteringValidator.Validate(criteria);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ToString());
        }
    }
}