using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IBankService
    {
        Task<BankDTO> GetBankById(int id);
    }
}
