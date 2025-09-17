using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class ClientRepository : Repository<Client> , IClientRepository
    {
        public ClientRepository( BankDbContext context ):base(context) { }
    }
}
