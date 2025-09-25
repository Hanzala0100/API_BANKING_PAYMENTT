using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;

namespace API_BANKING_PAYMENT.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IEmployeeRepository repository, IMapper mapper, ILogger<EmployeeService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseDTO<EmployeeDTO>> CreateAsync(EmployeeDTO model)
        {
            try
            {
                var existingEmployee = await _repository.GetByEmailAsync(model.Email) ?? await _repository.GetByUsernameAsync(model.UserName);
                if (existingEmployee != null)
                {
                    return BaseResponseDTO<EmployeeDTO>.ErrorResult("Employee with this email or username already exists.");
                }

                var newEmployee = _mapper.Map<Employee>(model);
                newEmployee.CreatedAt = DateTime.UtcNow;

                await _repository.Add(newEmployee);

                var createdDto = _mapper.Map<EmployeeDTO>(newEmployee);
                return BaseResponseDTO<EmployeeDTO>.SuccessResult(createdDto, "Employee created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return BaseResponseDTO<EmployeeDTO>.ErrorResult("Error creating employee", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteAsync(int id)
        {
            try
            {
                var existingEmployee = await _repository.GetById(id);
                if (existingEmployee == null)
                {
                    return BaseResponseDTO<bool>.ErrorResult("Employee not found.");
                }

                await _repository.Delete(entity: existingEmployee);

                return BaseResponseDTO<bool>.SuccessResult(true, "Employee deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee with ID {EmployeeId}", id);
                return BaseResponseDTO<bool>.ErrorResult("Error deleting employee", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<EmployeeDTO>>> GetAllAsync()
        {
            try
            {
                var employees = await _repository.GetAll();
                var employeeDtos = _mapper.Map<IEnumerable<EmployeeDTO>>(employees);

                return BaseResponseDTO<IEnumerable<EmployeeDTO>>.SuccessResult(
                    employeeDtos,
                    "Employees retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees");
                return BaseResponseDTO<IEnumerable<EmployeeDTO>>.ErrorResult(
                    "Error retrieving employees",
                    new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<EmployeeDTO>> GetByIdAsync(int id)
        {
            try
            {
                var employee = await _repository.GetById(id);
                if (employee == null)
                {
                    return BaseResponseDTO<EmployeeDTO>.ErrorResult("Employee not found.");
                }

                var employeeDto = _mapper.Map<EmployeeDTO>(employee);
                return BaseResponseDTO<EmployeeDTO>.SuccessResult(employeeDto, "Employee retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}", id);
                return BaseResponseDTO<EmployeeDTO>.ErrorResult("Error retrieving employee", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<EmployeeDTO>> UpdateAsync(int id, EmployeeDTO model)
        {
            try
            {
                var existingEmployee = await _repository.GetById(id);
                if (existingEmployee == null)
                {
                    return BaseResponseDTO<EmployeeDTO>.ErrorResult("Employee not found.");
                }

                _mapper.Map(model, existingEmployee);
                await _repository.Update(entity: existingEmployee);

                var updatedDto = _mapper.Map<EmployeeDTO>(existingEmployee);
                return BaseResponseDTO<EmployeeDTO>.SuccessResult(updatedDto, "Employee updated successfully!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee with ID {EmployeeId}", id);
                return BaseResponseDTO<EmployeeDTO>.ErrorResult("Error updating employee: ", new List<string> { ex.Message });
            }
        }
    }
}