using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IBankRepository : IRepository<Bank>
    {
        Task<Bank> GetBankWithDetails(long id);
        Task<Bank> GetBankByName(string Name);
        Task<List<Bank>> GetAllBanksAsync();


    }
}
