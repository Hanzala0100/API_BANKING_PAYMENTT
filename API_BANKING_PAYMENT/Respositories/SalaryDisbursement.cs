using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class SalaryDisbursement : Repository<SalaryDisbursement>, ISalaryDisbursement
    {
        public SalaryDisbursement(BankDbContext context) : base(context)
        {
        }
    }
}
