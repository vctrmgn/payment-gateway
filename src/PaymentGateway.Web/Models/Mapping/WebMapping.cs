using AutoMapper;
using PaymentGateway.Core.Dto;
using PaymentGateway.Web.Models.Payments;
using PaymentGateway.SharedKernel.Util;

namespace PaymentGateway.Web.Models.Mapping;

public class WebMapping : Profile
{
    public WebMapping()
    {
        CreateMap();
    }

    private void CreateMap()
    {
        CreateMap<CreditCard, CardInfoDto>();
        
        CreateMap<PaymentRequest, PaymentRequestDto>();
        
        CreateMap<PaymentResponseDto, PaymentResponse>();

        CreateMap<PaymentsFiltering, PaymentsFilteringDto>();
        
        CreateMap<PaymentDetailsDto, PaymentDetails>()
            .ForMember(dest => dest.PaymentStatus, mapper => mapper.MapFrom(source => source.Status.ToString()));
        
        CreateMap<PaginatedList<PaymentDetailsDto>, PaginatedList<PaymentDetails>>()
            .ConstructUsing((source, context) =>
                new PaginatedList<PaymentDetails>(
                    source.PageItems
                        .Select(p => context.Mapper.Map<PaymentDetails>(p)).ToList(),
                    source.Limit,
                    source.Skip,
                    source.TotalItems));
    }
}