using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;

using System.Formats.Asn1;

namespace API_BANKING_PAYMENT.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IClientRepository _clientRepository;

        public EmployeeService(IEmployeeRepository repository, IMapper mapper, ILogger<EmployeeService> logger, IClientRepository clientRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _clientRepository = clientRepository;
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

        //Import Bulk Employees

        public async Task<BaseResponseDTO<BulkEmployeeImportResponseDTO>> BulkImportEmployeesAsync(long clientId, IFormFile csvFile)
        {
            var response = new BulkEmployeeImportResponseDTO();

            try
            {
                if (csvFile == null || csvFile.Length == 0)
                    return BaseResponseDTO<BulkEmployeeImportResponseDTO>.ErrorResult("CSV file is required");

                if (Path.GetExtension(csvFile.FileName).ToLower() != ".csv")
                    return BaseResponseDTO<BulkEmployeeImportResponseDTO>.ErrorResult("Only CSV files are supported");

                // Validate client exists
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<BulkEmployeeImportResponseDTO>.ErrorResult("Client not found");

                var employees = await ParseCsvFileAsync(csvFile, clientId);
                response.TotalRecords = employees.Count;

                var validEmployees = new List<Employee>();
                var importedEmployees = new List<EmployeeDTO>();

                foreach (var employee in employees)
                {
                    try
                    {
                        // Validate required fields
                        if (string.IsNullOrEmpty(employee.FullName) ||
                            string.IsNullOrEmpty(employee.Email) ||
                            employee.AccountNumber == 0)
                        {
                            response.Errors.Add($"Missing required fields for employee: {employee.FullName}");
                            response.Failed++;
                            continue;
                        }

                        // Check if employee already exists
                        var exists = await _repository.EmployeeExistsAsync(clientId, employee.Email, employee.AccountNumber);
                        if (exists)
                        {
                            response.Errors.Add($"Employee already exists: {employee.Email} or Account: {employee.AccountNumber}");
                            response.Failed++;
                            continue;
                        }

                        // Validate email format
                        if (!IsValidEmail(employee.Email))
                        {
                            response.Errors.Add($"Invalid email format: {employee.Email}");
                            response.Failed++;
                            continue;
                        }

                        // Create employee entity
                        var employeeEntity = new Employee
                        {
                            ClientId = clientId,
                            FullName = employee.FullName.Trim(),
                            PhoneNumber = employee.PhoneNumber?.Trim() ?? string.Empty,
                            Email = employee.Email.Trim().ToLower(),
                            AccountNumber = employee.AccountNumber,
                            BankName = employee.BankName.Trim(),
                            Ifsccode = employee.Ifsccode.Trim(),
                            SalaryAmount = employee.SalaryAmount > 0 ? employee.SalaryAmount : 0,
                            CreatedAt = DateTime.UtcNow
                        };

                        validEmployees.Add(employeeEntity);
                        response.Successful++;
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Error processing employee {employee.FullName}: {ex.Message}");
                        response.Failed++;
                    }
                }

                // Bulk insert valid employees
                if (validEmployees.Any())
                {
                    await _repository.AddRangeAsync(validEmployees);

                    // Map to DTOs for response
                    importedEmployees = _mapper.Map<List<EmployeeDTO>>(validEmployees);
                    response.ImportedEmployees = importedEmployees;
                }

                _logger.LogInformation("Bulk employee import completed. Client: {ClientId}, Successful: {Successful}, Failed: {Failed}",
                    clientId, response.Successful, response.Failed);

                return BaseResponseDTO<BulkEmployeeImportResponseDTO>.SuccessResult(response,
                    $"Bulk import completed. Successful: {response.Successful}, Failed: {response.Failed}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk employee import for client ID: {ClientId}", clientId);
                return BaseResponseDTO<BulkEmployeeImportResponseDTO>.ErrorResult("Bulk import failed", new List<string> { ex.Message });
            }
        }

        private async Task<List<CsvEmployeeRecordDTO>> ParseCsvFileAsync(IFormFile csvFile, long clientId)
        {
            var employees = new List<CsvEmployeeRecordDTO>();

            using (var stream = new StreamReader(csvFile.OpenReadStream()))
            using (var csv = new CsvReader(stream, System.Globalization.CultureInfo.InvariantCulture))
            {
                // Configure CSV reader
                csv.Context.RegisterClassMap<CsvEmployeeMap>();

                var records = csv.GetRecords<CsvEmployeeRecordDTO>();
                employees = records.ToList();
            }

            return employees;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public sealed class CsvEmployeeMap : ClassMap<CsvEmployeeRecordDTO>
        {
            public CsvEmployeeMap()
            {
                Map(m => m.FullName).Name("FullName", "Employee Name", "Name");
                Map(m => m.PhoneNumber).Name("PhoneNumber", "Phone", "Mobile");
                Map(m => m.Email).Name("Email", "Email Address");
                Map(m => m.AccountNumber).Name("AccountNumber", "Account No", "Account");
                Map(m => m.BankName).Name("BankName", "Bank Name", "Bank");
                Map(m => m.Ifsccode).Name("Ifsccode", "IFSC Code", "IFSC");
                Map(m => m.SalaryAmount).Name("SalaryAmount", "Salary", "Amount");
            }
        }

        //Paginated Retrieval of Employees
        public async Task<BaseResponseDTO<PaginatedResponseDTO<EmployeeDTO>>> GetAllPaginatedAsync(PaginationRequestDTO paginationRequest)
        {
            try
            {
                // Validate client exists
                var client = await _clientRepository.GetById(paginationRequest.ClientId);
                if (client == null)
                {
                    return BaseResponseDTO<PaginatedResponseDTO<EmployeeDTO>>.ErrorResult("Client not found.");
                }

                var (employees, totalCount) = await _repository.GetPaginatedAsync(
                    paginationRequest.ClientId, 
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    paginationRequest.SearchTerm,
                    paginationRequest.SortBy,
                    paginationRequest.SortDescending);

                var employeeDtos = _mapper.Map<IEnumerable<EmployeeDTO>>(employees);

                var paginatedResponse = new PaginatedResponseDTO<EmployeeDTO>
                {
                    Data = employeeDtos,
                    Pagination = new PaginationMetadataDTO
                    {
                        CurrentPage = paginationRequest.PageNumber,
                        PageSize = paginationRequest.PageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)paginationRequest.PageSize)
                    },
                    Message = "Employees retrieved successfully.",
                    Success = true
                };

                return BaseResponseDTO<PaginatedResponseDTO<EmployeeDTO>>.SuccessResult(
                    paginatedResponse,
                    "Employees retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated employees for Client ID: {ClientId}", paginationRequest.ClientId);
                return BaseResponseDTO<PaginatedResponseDTO<EmployeeDTO>>.ErrorResult(
                    "Error retrieving employees",
                    new List<string> { ex.Message });
            }
        }
    }
}