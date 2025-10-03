using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class BeneficiaryRepository : Repository<Beneficiary>, IBeneficiaryRepository
    {
        private readonly BankDbContext _context;
        public BeneficiaryRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Beneficiary>> GetAllBeneficiariesByClientId(long Id)
        {
            return await _context.Beneficiaries
                .Where(b => b.ClientId == Id)
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

    public async Task<(IEnumerable<Beneficiary> Beneficiaries, int TotalCount)> GetPaginatedAsync(
      long clientId,
      int pageNumber,
      int pageSize,
      string? searchTerm = null,
      string? sortBy = null,
      bool sortDescending = false)
        {
            var query = _context.Beneficiaries
                .Where(b => b.ClientId == clientId)
                .Include(b => b.Payments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.FullName, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.BankName, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.Ifsccode, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.AccountNumber.ToString(), $"%{searchTerm}%"));
            }

            query = (sortBy?.ToLower()) switch
            {
                "fullname" => sortDescending ? query.OrderByDescending(b => b.FullName) : query.OrderBy(b => b.FullName),
                "createdat" => sortDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                "accountnumber" => sortDescending ? query.OrderByDescending(b => b.AccountNumber) : query.OrderBy(b => b.AccountNumber),
                "bankname" => sortDescending ? query.OrderByDescending(b => b.BankName) : query.OrderBy(b => b.BankName),
                "ifsccode" => sortDescending ? query.OrderByDescending(b => b.Ifsccode) : query.OrderBy(b => b.Ifsccode),
                _ => sortDescending ? query.OrderByDescending(b => b.BeneficiaryId) : query.OrderBy(b => b.BeneficiaryId)
            };

            var totalCount = await query.CountAsync();

            var beneficiaries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()  
                .ToListAsync();

            return (beneficiaries, totalCount);
        }
    }

}

