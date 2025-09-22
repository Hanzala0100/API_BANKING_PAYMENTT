using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Respositories;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;

namespace API_BANKING_PAYMENT.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public EmployeeService(IEmployeeRepository repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _mapper = mapper;
        }

        async Task<RegisterResponseModel> IEmployeeService.RegisterAsync(EmployeeDTO model)
        {
            var existingUser = await _repository.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new RegisterResponseModel
                {
                    IsSuccess = false,
                    Message = "User already exists with this email."
                };
            }

            return new RegisterResponseModel
            {
                IsSuccess = true,
                Message = "User registered successfully."
            };
        }
    }
}
