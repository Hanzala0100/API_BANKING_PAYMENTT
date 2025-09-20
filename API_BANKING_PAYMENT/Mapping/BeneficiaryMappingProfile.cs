using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class BeneficiaryMappingProfile : Profile
    {
        public BeneficiaryMappingProfile()
        {
            CreateMap<Beneficiary,BeneficiaryDTO>()
                .ForMember(dest => dest.TotalPayments, opt => opt.MapFrom(src => src.Payments.Count));
        }
    }
}
