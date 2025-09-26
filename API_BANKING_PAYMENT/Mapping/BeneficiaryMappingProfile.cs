using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class BeneficiaryMappingProfile : Profile
    {
        public BeneficiaryMappingProfile()
        {
            // Map from RequestDTO to Entity
            CreateMap<BeneficiaryRequestDTO, Beneficiary>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.BeneficiaryId, opt => opt.Ignore())  
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            // Map from Entity to ResponseDTO
            CreateMap<Beneficiary, BeneficiaryDTO>()
                .ForMember(dest => dest.TotalPayments, opt => opt.MapFrom(src => src.Payments.Count));
        }
    }
}