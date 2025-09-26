using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByClientIdAsync(long clientId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByBeneficiaryIdAsync(long beneficiaryId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Payment> GetPaymentWithDetailsAsync(long paymentId);
    }
}
