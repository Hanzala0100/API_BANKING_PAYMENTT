using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface ISalaryDisbursementRepository : IRepository<SalaryDisbursement>
    {
        Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByClientIdAsync(long clientId);
        Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByEmployeeIdAsync(long employeeId);
        Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByStatusAsync(string status);
        Task<IEnumerable<SalaryDisbursement>> GetPendingSalaryDisbursementsAsync();
        Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<SalaryDisbursement> GetSalaryDisbursementWithDetailsAsync(long salaryId);
        Task<bool> EmployeeHasPendingSalaryAsync(long employeeId, DateTime disbursementDate);
    }
}
