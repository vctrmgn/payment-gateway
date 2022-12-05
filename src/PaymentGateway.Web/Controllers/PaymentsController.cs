using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Core.Dto;
using PaymentGateway.Core.Interfaces.Services;
using PaymentGateway.SharedKernel.Enums;
using PaymentGateway.SharedKernel.Exceptions;
using PaymentGateway.SharedKernel.Util;
using PaymentGateway.Web.Filters;
using PaymentGateway.Web.Models.Constants;
using PaymentGateway.Web.Models.Payments;
using PaymentGateway.Web.Security;

namespace PaymentGateway.Web.Controllers;

[ApiController]
[Route("/api/v1/payments")]
[PaymentsExceptionFilter]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRetrievalService _paymentRetrievalService;
    private readonly IMapper _mapper;

    public PaymentsController(
        IPaymentService paymentService,
        IPaymentRetrievalService paymentRetrievalService,
        IMapper mapper)
    {
        _paymentService = paymentService;
        _paymentRetrievalService = paymentRetrievalService;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = Roles.PaymentsRequester)]
    public async Task<ActionResult<PaymentResponse>> ProcessPaymentAsync(
        [FromIdentity] UserIdentity identity,
        [FromHeader(Name = HeaderConstants.IdempotencyKeyHeaderName), Required] string idempotencyKey,
        [FromBody] PaymentRequest paymentRequest,
        CancellationToken cancellationToken)
    {
        var paymentRequestDto = _mapper.Map<PaymentRequestDto>(paymentRequest);
        
        var result = await _paymentService
            .ProcessPaymentAsync(identity.MerchantId, idempotencyKey, paymentRequestDto, cancellationToken);

        var paymentResponse = _mapper.Map<PaymentResponse>(result);
        
        if(result.PaymentStatus == PaymentStatus.Unknown)
            return Accepted(paymentResponse);
        
        return Created(paymentResponse.PaymentId.ToString(), paymentResponse);
    }

    [HttpGet]
    [Authorize(Roles = Roles.PaymentsReader)]
    public async Task<ActionResult<PaginatedList<PaymentDetails>>> GetPaymentDetailsAsync(
        [FromIdentity] UserIdentity identity,
        [FromQuery] PaymentsFiltering queryParams,
        CancellationToken cancellationToken)
    {
        var filteringCriteria = _mapper.Map<PaymentsFilteringDto>(queryParams);

        var paymentsDetails = await _paymentRetrievalService
            .ListPaymentsDetailsAsync(identity.MerchantId, filteringCriteria, cancellationToken);

        var response = _mapper.Map<PaginatedList<PaymentDetails>>(paymentsDetails);
        
        return Ok(response);
    }

    [HttpGet("{paymentId}")]
    [Authorize(Roles = Roles.PaymentsReader)]
    public async Task<ActionResult<PaymentDetails>> GetPaymentDetailsAsync(
        [FromIdentity] UserIdentity identity,
        [FromRoute] string paymentId,
        CancellationToken cancellationToken)
    {
        var paymentDetails = await _paymentRetrievalService
            .GetPaymentDetailsAsync(identity.MerchantId, paymentId, cancellationToken);

        var response = _mapper.Map<PaymentDetails>(paymentDetails);
        
        return Ok(response ?? throw new ResourceNotFoundException(paymentId));
    }
}