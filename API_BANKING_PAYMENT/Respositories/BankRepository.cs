using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class BankRepository : Repository<Bank>, IBankRepository
    {
        public BankRepository(BankDbContext context) : base(context)
        {
        }
    }
}
