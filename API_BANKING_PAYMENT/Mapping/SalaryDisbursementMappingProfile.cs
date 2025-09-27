using API_BANKING_PAYMENT.Models.DTO;
using AutoMapper;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Mapping
{
    public class SalaryDisbursementMappingProfile : Profile
    {
        public SalaryDisbursementMappingProfile()
        {
            CreateMap<SalaryDisbursement, SalaryDisbursementDTO>()
                .ForMember(dest => dest.EmployeeName,
                    opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.EmployeeAccountNumber,
                    opt => opt.MapFrom(src => src.Employee.AccountNumber))
                .ForMember(dest => dest.ClientName,
                    opt => opt.MapFrom(src => src.Client.ClientName));

            CreateMap<CreateSalaryDisbursementDTO, SalaryDisbursement>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.DisbursementDate, opt => opt.MapFrom(src => src.DisbursementDate));

            CreateMap<SalaryDisbursementDTO, SalaryDisbursement>();
        }
    }
}
