using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
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
        private readonly ILogger<EmployeeService> _logger;
        public EmployeeService(IEmployeeRepository repository, IConfiguration config, IMapper mapper, ILogger<EmployeeService> logger)
        {
            _repository = repository;
            _config = config;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingEmployee = await _repository.GetById(id);
            if (existingEmployee == null)
            {
                return false;
            }

            await _repository.Delete(entity: existingEmployee);
            return true;
        }

        public async Task<IEnumerable<EmployeeDTO>> GetAllAsync()
        {
            var employees = await _repository.GetAll();
            return _mapper.Map<IEnumerable<EmployeeDTO>>(employees);
        }

        public async Task<EmployeeDTO> GetByIdAsync(int id)
        {
            var existingEmployee = await _repository.GetById(id);
            if (existingEmployee == null)
            {
                return null;
            }

            return _mapper.Map<EmployeeDTO>(existingEmployee);
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
            var newEmployee = _mapper.Map<Employee>(model);
            newEmployee.CreatedAt = DateTime.UtcNow;   
            await _repository.Add(newEmployee);


            return new RegisterResponseModel
            {
                IsSuccess = true,
                Message = "User registered successfully."
            };
        }

        public async Task<bool> UpdateAsync(int id, EmployeeDTO model)
        {
            var existingEmployee = await _repository.GetById(id);
            if (existingEmployee == null)
            {
                return false; 
            }

            _mapper.Map(model, existingEmployee);


            await _repository.Update(entity: existingEmployee);

            return true; 
        }

    }
}
