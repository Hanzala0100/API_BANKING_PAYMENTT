using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IUserService
    {
        Task<LoginResponseModel> LoginAsync(LoginViewModel user);
        Task<LoginResponseModel> RegisterAsync(RegisterDTO model);
    }
}
