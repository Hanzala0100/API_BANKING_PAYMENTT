using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.DTO;
using AutoMapper;

namespace API_BANKING_PAYMENT.MappingProfiles
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            // Request -> Entity
            CreateMap<ReportRequestDTO, Report>()
                .ForMember(dest => dest.GeneratedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ReportId, opt => opt.Ignore())
                .ForMember(dest => dest.GeneratedByNavigation, opt => opt.Ignore());

            // Entity -> DTO 
            CreateMap<Report, ReportDTO>()
                .ForMember(dest => dest.GeneratedByName, opt => opt.MapFrom(src => src.GeneratedByNavigation.FullName));
        }
    }
}