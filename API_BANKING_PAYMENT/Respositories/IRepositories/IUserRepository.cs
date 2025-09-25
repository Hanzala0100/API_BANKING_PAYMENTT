using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IUserRepository :IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task AddClientAsync(Client client);
        Task<IEnumerable<User>> GetUsersByBankId(long bankId);
        Task<User> GetBankUserBankId(long bankId);
    }
}
