using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly BankDbContext _context;
        public UserRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Bank)
                .FirstOrDefaultAsync(u => u.Email == email);

        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> AddClientUser(User clientUser)
        {
            _context.Users.Add(clientUser);
            await _context.SaveChangesAsync();
            return clientUser;
        }

        public async Task<IEnumerable<User>> GetUsersByBankId(long bankId)
        {
            return await _context.Users
                .Where(usr => usr.BankId == bankId)
                .ToListAsync();
        }
        public async Task<IEnumerable<User>> GetUsersByClientId(long clientId)
        {
            return await _context.Users
                .Where(usr => usr.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<User> GetBankUserBankId(long bankId)
        {
             return await _context.Users
                    .FirstOrDefaultAsync(u => u.BankId == bankId);
        }
    }
}
