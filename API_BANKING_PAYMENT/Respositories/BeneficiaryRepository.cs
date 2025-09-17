using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class BeneficiaryRepository : Repository<Beneficiary> , IBeneficiaryRepository
    {
        public BeneficiaryRepository(BankDbContext context) : base(context)
        {
        }
    }
    
}
