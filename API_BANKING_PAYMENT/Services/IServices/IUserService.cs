using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IUserService
    {
        Task<LoginResponseModel> LoginAsync(LoginViewModel user);
    }
}
