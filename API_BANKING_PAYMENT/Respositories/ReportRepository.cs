using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        private readonly BankDbContext _context;
        public ReportRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Report>> GetReportsByUserIdAsync(long userId)
        {
            return await _context.Reports
                .Where(r => r.GeneratedBy == userId)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }
        public async Task<Report?> GetByIdWithUserAsync(long id)
        {
            return await _context.Reports
                .Include(r => r.GeneratedByNavigation) // include navigation
                .FirstOrDefaultAsync(r => r.ReportId == id);
        }


    }
}
