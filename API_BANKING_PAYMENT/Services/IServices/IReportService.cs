using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IReportService
    {
        Task<BaseResponseDTO<ReportDTO>> GenerateAndUploadReportAsync();
        Task<BaseResponseDTO<ReportDTO>> GetReportByIdAsync(long reportId);
        Task<BaseResponseDTO<IEnumerable<ReportDTO>>> GetReportsForCurrentUserAsync();
        Task<BaseResponseDTO<bool>> DeleteReportAsync(long reportId);
        Task<byte[]> GenerateClientReportAsync(long clientId);
        Task<byte[]> GenerateBankReportAsync(long bankId);
        Task<byte[]> GenerateSuperAdminReportAsync();
    }
}
