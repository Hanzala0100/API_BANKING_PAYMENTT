using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly BankDbContext _context;

        public PaymentRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByClientIdAsync(long clientId)
        {
            return await _context.Payments
                .Where(p => p.ClientId == clientId)
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .Include(p => p.ApprovedByNavigation)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByBankUserIdAsync(long BankId)
        {
            return await _context.Payments
                .Where(p => p.Client.BankId == BankId)
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .Include(p => p.ApprovedByNavigation)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .Include(p => p.ApprovedByNavigation)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == "Pending")
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByBeneficiaryIdAsync(long beneficiaryId)
        {
            return await _context.Payments
                .Where(p => p.BeneficiaryId == beneficiaryId)
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .Include(p => p.ApprovedByNavigation)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .Include(p => p.ApprovedByNavigation)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> GetPaymentWithDetailsAsync(long paymentId)
        {
            return await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .Include(p => p.Beneficiary)
                .Include(p => p.Client)
                .Include(p => p.ApprovedByNavigation)
                .FirstOrDefaultAsync();
        }
    }
}