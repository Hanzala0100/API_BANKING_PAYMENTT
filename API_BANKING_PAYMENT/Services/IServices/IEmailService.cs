using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IEmailService
    {
        Task<BaseResponseDTO<bool>> SendEmailAsync(EmailRequestDTO emailRequest);
        Task<BaseResponseDTO<bool>> SendClientVerificationEmailAsync(ClientEmailRequestDTO clientEmailRequest);
        Task<BaseResponseDTO<bool>> SendBulkEmailsAsync(List<EmailRequestDTO> emailRequests);
        Task<BaseResponseDTO<bool>> SendClientUserWelcomeEmailAsync(ClientUserCreationDTO clientUser);
        Task<BaseResponseDTO<bool>> SendPendingVerificationEmailAsync(long clientId, string userEmail);
        Task<BaseResponseDTO<bool>> SendApprovalEmailAsync(long clientId, string notes);
        Task<BaseResponseDTO<bool>> SendRejectionEmailAsync(long clientId, string reasons);
    }
}