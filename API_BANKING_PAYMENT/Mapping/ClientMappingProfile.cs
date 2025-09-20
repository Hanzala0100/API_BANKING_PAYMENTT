using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<Client, ClientDTO>()
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank.BankName))
                .ForMember(dest => dest.TotalEmployees, opt => opt.MapFrom(src => src.Employees.Count))
                .ForMember(dest => dest.TotalBeneficiaries, opt => opt.MapFrom(src => src.Beneficiaries.Count))
                .ForMember(dest => dest.TotalPayments, opt => opt.MapFrom(src => src.Payments.Count));
        }
    }
}
