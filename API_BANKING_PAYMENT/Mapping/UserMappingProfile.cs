using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.DTO;
using AutoMapper;
using System;

namespace API_BANKING_PAYMENT.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.BankName,opt => opt.MapFrom(src => src.Bank != null ? src.Bank.BankName : null))
                .ForMember(dest => dest.ClientName,opt => opt.MapFrom(src => src.Client != null ? src.Client.ClientName : null));


            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
