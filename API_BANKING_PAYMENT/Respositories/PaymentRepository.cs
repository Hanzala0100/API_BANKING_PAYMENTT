using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(BankDbContext context) : base(context)
        {
        }
    }
}
