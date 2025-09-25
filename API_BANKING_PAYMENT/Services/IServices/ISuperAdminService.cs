using API_BANKING_PAYMENT.Models.DTO;

public interface ISuperAdminService
{
    // Bank   
    Task<BaseResponseDTO<BankDTO>> CreateBankAsync(BankCreationDTO bankCreationDTO);
    Task<BaseResponseDTO<BankDTO>> UpdateBankAsync(BankDTO bankDTO);
    Task<BaseResponseDTO<bool>> DeleteBankAsync(long bankId);
    Task<BaseResponseDTO<BankDTO>> GetBankByIdAsync(long bankId);
    Task<BaseResponseDTO<IEnumerable<BankDTO>>> GetAllBanksAsync();

    // Report Generation
    Task<BaseResponseDTO<ReportDTO>> GenerateSystemUsageReportAsync(ReportRequestDTO request);
    Task<BaseResponseDTO<ReportDTO>> GenerateAuditLogReportAsync(ReportRequestDTO request);
}