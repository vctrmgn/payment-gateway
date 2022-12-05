using AutoMapper;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Dto.Mapping;

public class EntityToDtoMapper : Profile
{
    public EntityToDtoMapper()
    {
        CreateMap();
    }

    private void CreateMap()
    {
        CreateMap<Payment, PaymentDetailsDto>()
            .ForMember(dest => dest.PaymentId, map => map.MapFrom(source => source.Id.ToString()))
            .ForMember(dest => dest.CardHolderName, map => map.MapFrom(source => source.MaskedCardInfo.HolderName))
            .ForMember(dest => dest.CardNumber, map => map.MapFrom(source => source.MaskedCardInfo.Number))
            .ForMember(dest => dest.CardExpiryMonth, map => map.MapFrom(source => source.MaskedCardInfo.ExpiryMonth))
            .ForMember(dest => dest.CardExpiryYear, map => map.MapFrom(source => source.MaskedCardInfo.ExpiryYear))
            .ForMember(dest => dest.Status, map => map.MapFrom(source => source.Status.ToString()));

        CreateMap<Payment, PaymentBankResponseDto>();
    }
}