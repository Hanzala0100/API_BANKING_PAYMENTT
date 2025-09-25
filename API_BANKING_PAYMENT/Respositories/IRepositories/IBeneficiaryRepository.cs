using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IBeneficiaryRepository : IRepository<Beneficiary>
    {
        Task<Beneficiary> GetBeneficiaryById(long Id);
        Task<IEnumerable<Beneficiary>> GetAllBeneficiariesByClientId(long Id);
        Task<Beneficiary> GetByClientAndAccountAsync(long clientId, long accountNumber, string ifscCode);
    }
}
