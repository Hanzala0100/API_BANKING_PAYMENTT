using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class ClientRepository : Repository<Client> , IClientRepository
    {
        private readonly BankDbContext _context;
        public ClientRepository( BankDbContext context ):base(context) { 
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetClientsByBankId(long bankId)
        {
            return await _context.Clients
                .Where(cl => cl.BankId == bankId)
               .ToListAsync();
        }


    }
}
