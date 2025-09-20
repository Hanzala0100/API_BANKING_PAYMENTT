using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Payment, PaymentDTO>()
        .ForMember(dest => dest.ClientName,
            opt => opt.MapFrom(src => src.Client != null ? src.Client.ClientName : null))
        .ForMember(dest => dest.BeneficiaryName,
            opt => opt.MapFrom(src => src.Beneficiary != null ? src.Beneficiary.FullName : null));
        }

    }
}
