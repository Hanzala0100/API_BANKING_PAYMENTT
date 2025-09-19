using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

namespace API_BANKING_PAYMENT.Mapping
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.BankName : null));
        }
    }
}
