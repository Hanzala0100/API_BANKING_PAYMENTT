using API_BANKING_PAYMENT.Models.DTO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IDocumentService
{
    Task<BaseResponseDTO<DocumentDTO>> UploadDocumentAsync(
        IFormFile file,
        long uploadedBy,
        long bankId,
        long? clientId = null,
        string? docType = null
    );

    Task<BaseResponseDTO<bool>> DeleteDocumentAsync(long documentId);
    Task<BaseResponseDTO<DocumentDTO>> GetDocumentByIdAsync(long documentId);
    Task<BaseResponseDTO<DocumentDTO>> UpdateDocumentAsync(long documentId, IFormFile newFile);
}

