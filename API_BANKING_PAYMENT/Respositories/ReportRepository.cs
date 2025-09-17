using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository(BankDbContext context) : base(context)
        {
        }
    }
    {
    }
}
