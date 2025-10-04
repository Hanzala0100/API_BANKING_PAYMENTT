using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;

namespace API_BANKING_PAYMENT.Services
{
    public class SalaryDisbursementService : ISalaryDisbursementService
    {
        private readonly ISalaryDisbursementRepository _salaryDisbursementRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SalaryDisbursementService> _logger;

        public SalaryDisbursementService(
            ISalaryDisbursementRepository salaryDisbursementRepository,
            IEmployeeRepository employeeRepository,
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<SalaryDisbursementService> logger)
        {
            _salaryDisbursementRepository = salaryDisbursementRepository;
            _employeeRepository = employeeRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseDTO<SalaryDisbursementDTO>> CreateSalaryDisbursementAsync(CreateSalaryDisbursementDTO disbursementDTO)
        {
            try
            {
                if (disbursementDTO == null)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Salary disbursement data is required");

                if (disbursementDTO.Amount <= 0)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Amount must be greater than zero");

                var employee = await _employeeRepository.GetById(disbursementDTO.EmployeeId);
                if (employee == null || employee.ClientId != disbursementDTO.ClientId)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Invalid employee");

                var client = await _clientRepository.GetById(disbursementDTO.ClientId);
                if (client == null)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Invalid client");

                var hasPendingSalary = await _salaryDisbursementRepository.EmployeeHasPendingSalaryAsync(
                    disbursementDTO.EmployeeId, disbursementDTO.DisbursementDate);

                if (hasPendingSalary)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Salary already disbursed or pending for this month");

                var salaryDisbursement = _mapper.Map<SalaryDisbursement>(disbursementDTO);
                salaryDisbursement.Status = "Completed";
                salaryDisbursement.CreatedAt = DateTime.UtcNow;

                await _salaryDisbursementRepository.Add(salaryDisbursement);

                var salaryDTO = _mapper.Map<SalaryDisbursementDTO>(salaryDisbursement);
                _logger.LogInformation("Salary disbursement created. SalaryId: {SalaryId}, EmployeeId: {EmployeeId}",
                    salaryDisbursement.SalaryId, disbursementDTO.EmployeeId);

                return BaseResponseDTO<SalaryDisbursementDTO>.SuccessResult(salaryDTO, "Salary disbursement created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating salary disbursement for EmployeeId: {EmployeeId}", disbursementDTO?.EmployeeId);
                return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Failed to create salary disbursement", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<BatchSalaryDisbursementResponseDTO>> CreateBatchSalaryDisbursementAsync(BatchSalaryDisbursementDTO batchDTO)
        {
            var response = new BatchSalaryDisbursementResponseDTO();

            try
            {
                if (batchDTO == null || !batchDTO.Employees.Any())
                    return BaseResponseDTO<BatchSalaryDisbursementResponseDTO>.ErrorResult("Batch data is required");

                var client = await _clientRepository.GetById(batchDTO.ClientId);
                if (client == null)
                    return BaseResponseDTO<BatchSalaryDisbursementResponseDTO>.ErrorResult("Invalid client");

                foreach (var employeeSalary in batchDTO.Employees)
                {
                    try
                    {
                        var employee = await _employeeRepository.GetById(employeeSalary.EmployeeId);
                        if (employee == null || employee.ClientId != batchDTO.ClientId)
                        {
                            response.Errors.Add($"Invalid employee ID: {employeeSalary.EmployeeId}");
                            response.Failed++;
                            continue;
                        }

                        if (employeeSalary.Amount <= 0)
                        {
                            response.Errors.Add($"Invalid amount for employee: {employee.FullName}");
                            response.Failed++;
                            continue;
                        }

                        var hasPendingSalary = await _salaryDisbursementRepository.EmployeeHasPendingSalaryAsync(
                            employeeSalary.EmployeeId, batchDTO.DisbursementDate);

                        if (hasPendingSalary)
                        {
                            response.Errors.Add($"Salary already disbursed for employee: {employee.FullName}");
                            response.Failed++;
                            continue;
                        }

                        var salaryDisbursement = new SalaryDisbursement
                        {
                            ClientId = batchDTO.ClientId,
                            EmployeeId = employeeSalary.EmployeeId,
                            Amount = employeeSalary.Amount,  
                            Status = "Completed",
                            DisbursementDate = batchDTO.DisbursementDate,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _salaryDisbursementRepository.Add(salaryDisbursement);

                        var salaryDTO = _mapper.Map<SalaryDisbursementDTO>(salaryDisbursement);
                        response.ProcessedSalaries.Add(salaryDTO);
                        response.Successful++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing salary for EmployeeId: {EmployeeId}", employeeSalary.EmployeeId);
                        response.Errors.Add($"Failed to process salary for employee ID: {employeeSalary.EmployeeId}");
                        response.Failed++;
                    }
                }

                response.TotalProcessed = response.Successful + response.Failed;

                _logger.LogInformation("Batch salary disbursement completed. Successful: {Successful}, Failed: {Failed}",
                    response.Successful, response.Failed);

                return BaseResponseDTO<BatchSalaryDisbursementResponseDTO>.SuccessResult(
                    response, $"Batch salary disbursement completed. Successful: {response.Successful}, Failed: {response.Failed}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch salary disbursement for ClientId: {ClientId}", batchDTO?.ClientId);
                return BaseResponseDTO<BatchSalaryDisbursementResponseDTO>.ErrorResult(
                    "Failed to process batch salary disbursement", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<SalaryDisbursementDTO>> GetSalaryDisbursementByIdAsync(long salaryId)
        {
            try
            {
                var salaryDisbursement = await _salaryDisbursementRepository.GetSalaryDisbursementWithDetailsAsync(salaryId);
                if (salaryDisbursement == null)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Salary disbursement not found");

                var salaryDTO = _mapper.Map<SalaryDisbursementDTO>(salaryDisbursement);
                return BaseResponseDTO<SalaryDisbursementDTO>.SuccessResult(salaryDTO, "Salary disbursement retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary disbursement with ID: {SalaryId}", salaryId);
                return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Failed to retrieve salary disbursement", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>> GetSalaryDisbursementsByClientIdAsync(long clientId)
        {
            try
            {
                var salaryDisbursements = await _salaryDisbursementRepository.GetSalaryDisbursementsByClientIdAsync(clientId);
                var salaryDTOs = _mapper.Map<IEnumerable<SalaryDisbursementDTO>>(salaryDisbursements);

                return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.SuccessResult(salaryDTOs, "Salary disbursements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary disbursements for ClientId: {ClientId}", clientId);
                return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.ErrorResult("Failed to retrieve salary disbursements", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>> GetSalaryDisbursementsByEmployeeIdAsync(long employeeId)
        {
            try
            {
                var salaryDisbursements = await _salaryDisbursementRepository.GetSalaryDisbursementsByEmployeeIdAsync(employeeId);
                var salaryDTOs = _mapper.Map<IEnumerable<SalaryDisbursementDTO>>(salaryDisbursements);

                return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.SuccessResult(salaryDTOs, "Salary disbursements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary disbursements for EmployeeId: {EmployeeId}", employeeId);
                return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.ErrorResult("Failed to retrieve salary disbursements", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<SalaryDisbursementDTO>> ProcessSalaryDisbursementAsync(long salaryId)
        {
            try
            {
                var salaryDisbursement = await _salaryDisbursementRepository.GetById(salaryId);
                if (salaryDisbursement == null)
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Salary disbursement not found");

                if (salaryDisbursement.Status != "Pending")
                    return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult($"Salary disbursement is already {salaryDisbursement.Status}");

                // salary processing (we can put here the payment gateway integration logic , but afterwards if possible hehe)
                salaryDisbursement.Status = "Processing";
                await _salaryDisbursementRepository.Update(salaryDisbursement);

                await Task.Delay(1000);

                salaryDisbursement.Status = "Completed";
                await _salaryDisbursementRepository.Update(salaryDisbursement);

                var salaryDTO = _mapper.Map<SalaryDisbursementDTO>(salaryDisbursement);
                _logger.LogInformation("Salary disbursement processed successfully. SalaryId: {SalaryId}", salaryId);

                return BaseResponseDTO<SalaryDisbursementDTO>.SuccessResult(salaryDTO, "Salary disbursement processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing salary disbursement with ID: {SalaryId}", salaryId);

                try
                {
                    var salaryDisbursement = await _salaryDisbursementRepository.GetById(salaryId);
                    if (salaryDisbursement != null)
                    {
                        salaryDisbursement.Status = "Failed";
                        await _salaryDisbursementRepository.Update(salaryDisbursement);
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Error updating salary disbursement status to failed");
                }

                return BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Failed to process salary disbursement", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteSalaryDisbursementAsync(long salaryId)
        {
            try
            {
                var salaryDisbursement = await _salaryDisbursementRepository.GetById(salaryId);
                if (salaryDisbursement == null)
                    return BaseResponseDTO<bool>.ErrorResult("Salary disbursement not found");

                if (salaryDisbursement.Status == "Completed" || salaryDisbursement.Status == "Processing")
                    return BaseResponseDTO<bool>.ErrorResult($"Cannot delete {salaryDisbursement.Status} salary disbursement");

                await _salaryDisbursementRepository.Delete(salaryDisbursement);

                _logger.LogInformation("Salary disbursement deleted. SalaryId: {SalaryId}", salaryId);
                return BaseResponseDTO<bool>.SuccessResult(true, "Salary disbursement deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting salary disbursement with ID: {SalaryId}", salaryId);
                return BaseResponseDTO<bool>.ErrorResult("Failed to delete salary disbursement", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>> GetSalaryDisbursementsByStatusAsync(string status)
        {
            try
            {
                var validStatuses = new[] { "Pending", "Processing", "Completed", "Failed" };
                if (!validStatuses.Contains(status))
                    return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.ErrorResult("Invalid salary disbursement status");

                var salaryDisbursements = await _salaryDisbursementRepository.GetSalaryDisbursementsByStatusAsync(status);
                var salaryDTOs = _mapper.Map<IEnumerable<SalaryDisbursementDTO>>(salaryDisbursements);

                return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.SuccessResult(salaryDTOs, $"{status} salary disbursements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary disbursements with status: {Status}", status);
                return BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.ErrorResult("Failed to retrieve salary disbursements", new List<string> { ex.Message });
            }
        }
    }
}