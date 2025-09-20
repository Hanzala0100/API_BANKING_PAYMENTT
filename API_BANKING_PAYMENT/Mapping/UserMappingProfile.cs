using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.DTO;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.BankName : null));
        }
    }
}
