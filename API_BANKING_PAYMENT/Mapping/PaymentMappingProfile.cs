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
                .ForMember(dest => dest.BeneficiaryName,
                    opt => opt.MapFrom(src => src.Beneficiary.FullName))
                .ForMember(dest => dest.BeneficiaryAccountNumber,
                    opt => opt.MapFrom(src => src.Beneficiary.AccountNumber))
                .ForMember(dest => dest.ApprovedByName,
                    opt => opt.MapFrom(src => src.ApprovedByNavigation.FullName))
                .ForMember(dest => dest.ClientName,
                    opt => opt.MapFrom(src => src.Client.ClientName));

            CreateMap<CreatePaymentDTO, Payment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate));

            CreateMap<PaymentDTO, Payment>();
        }
    }
}
