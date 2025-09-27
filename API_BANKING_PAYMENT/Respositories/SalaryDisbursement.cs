using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class SalaryDisbursementRepository : Repository<SalaryDisbursement>, ISalaryDisbursementRepository
    {
        private readonly BankDbContext _context;

        public SalaryDisbursementRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByClientIdAsync(long clientId)
        {
            return await _context.SalaryDisbursements
                .Where(s => s.ClientId == clientId)
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByEmployeeIdAsync(long employeeId)
        {
            return await _context.SalaryDisbursements
                .Where(s => s.EmployeeId == employeeId)
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .OrderByDescending(s => s.DisbursementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByStatusAsync(string status)
        {
            return await _context.SalaryDisbursements
                .Where(s => s.Status == status)
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalaryDisbursement>> GetPendingSalaryDisbursementsAsync()
        {
            return await _context.SalaryDisbursements
                .Where(s => s.Status == "Pending")
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalaryDisbursement>> GetSalaryDisbursementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.SalaryDisbursements
                .Where(s => s.DisbursementDate >= startDate && s.DisbursementDate <= endDate)
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .OrderByDescending(s => s.DisbursementDate)
                .ToListAsync();
        }

        public async Task<SalaryDisbursement> GetSalaryDisbursementWithDetailsAsync(long salaryId)
        {
            return await _context.SalaryDisbursements
                .Where(s => s.SalaryId == salaryId)
                .Include(s => s.Employee)
                .Include(s => s.Client)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EmployeeHasPendingSalaryAsync(long employeeId, DateTime disbursementDate)
        {
            return await _context.SalaryDisbursements
                .AnyAsync(s => s.EmployeeId == employeeId &&
                              s.DisbursementDate.Month == disbursementDate.Month &&
                              s.DisbursementDate.Year == disbursementDate.Year &&
                              (s.Status == "Pending" || s.Status == "Processing"));
        }
    }
}