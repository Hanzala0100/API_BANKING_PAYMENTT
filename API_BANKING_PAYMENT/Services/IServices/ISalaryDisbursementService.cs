using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface ISalaryDisbursementService
    {
        Task<BaseResponseDTO<SalaryDisbursementDTO>> CreateSalaryDisbursementAsync(CreateSalaryDisbursementDTO disbursementDTO);
        Task<BaseResponseDTO<BatchSalaryDisbursementResponseDTO>> CreateBatchSalaryDisbursementAsync(BatchSalaryDisbursementDTO batchDTO);
        Task<BaseResponseDTO<SalaryDisbursementDTO>> GetSalaryDisbursementByIdAsync(long salaryId);
        Task<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>> GetSalaryDisbursementsByClientIdAsync(long clientId);
        Task<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>> GetSalaryDisbursementsByEmployeeIdAsync(long employeeId);
        Task<BaseResponseDTO<SalaryDisbursementDTO>> ProcessSalaryDisbursementAsync(long salaryId);
        Task<BaseResponseDTO<bool>> DeleteSalaryDisbursementAsync(long salaryId);
        Task<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>> GetSalaryDisbursementsByStatusAsync(string status);
    }

   
}
