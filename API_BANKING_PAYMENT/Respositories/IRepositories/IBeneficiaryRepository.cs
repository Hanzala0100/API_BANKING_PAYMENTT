using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IBeneficiaryRepository : IRepository<Beneficiary>
    {
        Task<Beneficiary> GetBeneficiaryById(long Id);
        Task<IEnumerable<Beneficiary>> GetAllBeneficiariesByClientId(long Id);
        Task<Beneficiary> GetBeneficiaryByAccountNumber(long clientId, long accountNumber);
        Task<(IEnumerable<Beneficiary> Beneficiaries, int TotalCount)> GetPaginatedAsync(
            long clientId,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false);
    }
}
