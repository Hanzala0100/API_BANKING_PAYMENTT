using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IUserContext
    {
        User GetCurrentUser();
    }

}
