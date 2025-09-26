using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IBeneficiaryService
    {
        Task<BaseResponseDTO<BeneficiaryDTO>> CreateAsync(BeneficiaryRequestDTO model);
        Task<BaseResponseDTO<bool>> DeleteAsync(long id);
        Task<BaseResponseDTO<BeneficiaryDTO>> GetByIdAsync(long id);
        Task<BaseResponseDTO<List<BeneficiaryDTO>>> GetByClientIdAsync(long clientId);
        Task<BaseResponseDTO<BeneficiaryDTO>> UpdateAsync(long id, BeneficiaryDTO model);

    }
}