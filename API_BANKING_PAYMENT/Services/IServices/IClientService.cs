using API_BANKING_PAYMENT.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IClientService
    {
        Task<BaseResponseDTO<DocumentDTO>> UploadClientDocumentAsync(long clientId, IFormFile file, long uploadedBy, string docType);
        Task<BaseResponseDTO<IEnumerable<DocumentDTO>>> GetClientDocumentsAsync(long clientId);
        Task<BaseResponseDTO<DocumentDTO>> GetClientDocumentByIdAsync(long clientId, long documentId);
        Task<BaseResponseDTO<bool>> DeleteClientDocumentAsync(long clientId, long documentId);
        Task<BaseResponseDTO<DocumentDTO>> UpdateClientDocumentAsync(long clientId, long documentId, IFormFile newFile);
        Task<BaseResponseDTO<IEnumerable<DocumentDTO>>> GetClientDocumentsByTypeAsync(long clientId, string docType);
    }
}