using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(BankDbContext context) : base(context)
        {
        }
    }
}
