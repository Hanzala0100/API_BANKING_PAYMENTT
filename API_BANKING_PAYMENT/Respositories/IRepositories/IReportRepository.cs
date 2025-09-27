using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<Report?> GetByIdWithUserAsync(long id);
        Task<IEnumerable<Report>> GetReportsByUserIdAsync(long userId);

    }
}
