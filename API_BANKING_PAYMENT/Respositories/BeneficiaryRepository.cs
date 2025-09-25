using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class BeneficiaryRepository : Repository<Beneficiary> , IBeneficiaryRepository
    {
        private readonly BankDbContext _context;
        public BeneficiaryRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BeneficiaryDTO>> GetAllBeneficiariesByClientId(long clientId)
        {
            return await _context.Beneficiaries
                .Where(b => b.ClientId == clientId)
                .Select(b => new BeneficiaryDTO
                {
                    BeneficiaryId = b.BeneficiaryId,
                    FullName = b.FullName,
                    AccountNumber = b.AccountNumber,
                    BankName = b.BankName,
                    Ifsccode = b.Ifsccode
                })
                .ToListAsync();
        }
      

        public async Task<Beneficiary> GetBeneficiaryById(long id)
        {
            return await _context.Beneficiaries
                .FirstOrDefaultAsync(b => b.BeneficiaryId == id);
        }

        public async Task<Beneficiary> GetBeneficiaryByAccountNumber(long clientId, long accountNumber)
        {
            return await _context.Beneficiaries
                .FirstOrDefaultAsync(b => b.ClientId == clientId && b.AccountNumber == accountNumber);
        }


    }

}
