using API_BANKING_PAYMENT.Models.DTO;

public interface IDocumentService
{
    Task<DocumentDTO> UploadDocumentAsync(IFormFile file, long uploadedBy, long bankId, long? clientId, string? docType);
}