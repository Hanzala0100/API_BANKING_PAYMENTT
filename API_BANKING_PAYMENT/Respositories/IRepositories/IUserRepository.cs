using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IUserRepository :IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task<User> AddClientUser(User clientUser);
        Task<IEnumerable<User>> GetUsersByBankId(long bankId);
        Task<IEnumerable<User>> GetUsersByClientId(long clientId);
        Task<User> GetBankUserBankId(long bankId);
        Task<User> GetClientById(long id);
    }
}
