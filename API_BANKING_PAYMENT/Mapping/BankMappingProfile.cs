using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class BankMappingProfile :Profile
    {
        public BankMappingProfile()
        {
            CreateMap<Bank,BankDTO>()
                .ForMember(dest => dest.TotalClients, opt => opt.MapFrom(src => src.Clients.Count))
                .ForMember(dest => dest.TotalUsers, opt => opt.MapFrom(src => src.Users.Count));
        }
    }
}
