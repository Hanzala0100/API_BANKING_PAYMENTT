using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IEmployeeService
    {
        Task<BaseResponseDTO<EmployeeDTO>> CreateAsync(EmployeeDTO model);
        Task<BaseResponseDTO<EmployeeDTO>> GetByIdAsync(int id);
        Task<BaseResponseDTO<IEnumerable<EmployeeDTO>>> GetAllAsync();
        Task<BaseResponseDTO<EmployeeDTO>> UpdateAsync(int id, EmployeeDTO model);
        Task<BaseResponseDTO<bool>> DeleteAsync(int id);
        Task<BaseResponseDTO<BulkEmployeeImportResponseDTO>> BulkImportEmployeesAsync(long clientId, IFormFile csvFile);
    }
}