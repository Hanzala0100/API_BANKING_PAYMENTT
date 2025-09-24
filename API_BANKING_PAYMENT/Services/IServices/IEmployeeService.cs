using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IEmployeeService
    {
        Task<RegisterResponseModel> RegisterAsync(EmployeeDTO model);
        Task<bool> UpdateAsync(int id, EmployeeDTO model);
        Task<bool> DeleteAsync(int id);
        Task<EmployeeDTO> GetByIdAsync(int id);
        Task<IEnumerable<EmployeeDTO>> GetAllAsync();
    }
}
