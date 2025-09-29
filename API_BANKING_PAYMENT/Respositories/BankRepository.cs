using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class BankRepository : Repository<Bank>, IBankRepository
    {
        private readonly BankDbContext _context;
        public BankRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Bank> GetBankWithDetails(int id)
        {
            return await _context.Banks
                .Include(b => b.Clients)
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.BankId == id);
        }

        public async Task<Bank> GetBankByName(string Name)
        {
            return await _context.Banks.FirstOrDefaultAsync(b => b.BankName == Name);
        }

        public async Task<List<Bank>> GetAllBanksAsync()
        {
            return await _context.Banks
                .Include(b => b.Clients)
                .Include(b => b.Users)
                .ToListAsync();
        }




    }
}
