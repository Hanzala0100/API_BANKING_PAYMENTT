using API_BANKING_PAYMENT.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IBankUserService
    {
        // Client  
        Task<BaseResponseDTO<ClientDTO>> CreateClientAsync(ClientCreationDTO clientDTO);
        Task<BaseResponseDTO<ClientDTO>> GetClientByIdAsync(long clientId);
        Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetAllClientsAsync(long id);
        Task<BaseResponseDTO<ClientDTO>> UpdateClientAsync(ClientDTO clientDTO);
        Task<BaseResponseDTO<bool>> DeleteClientAsync(long clientId);

        // Client Verification  
        Task<BaseResponseDTO<ClientDTO>> VerifyClientAsync(long clientId, long verifiedBy, long bankId, string verificationStatus, string notes);
        Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetClientsByVerificationStatusAsync(string verificationStatus);
        Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetClientsWithPendingVerificationAsync();

        // Document  
        Task<BaseResponseDTO<DocumentDTO>> UploadClientDocumentAsync(long clientId, IFormFile file, long uploadedBy, long bankId, string docType);
        Task<BaseResponseDTO<IEnumerable<DocumentDTO>>> GetClientDocumentsAsync(long clientId);

        // Client User  
        Task<BaseResponseDTO<ClientUserCreationDTO>> CreateClientUserAsync(RegisterDTO userDTO);
        Task<BaseResponseDTO<UserDTO>> GetClienUserByIdAsync(long clientUserId);
        Task<BaseResponseDTO<IEnumerable<UserDTO>>> GetAllClientUsersByClientIdAsync(long clientId);
        Task<BaseResponseDTO<bool>> DeleteClientUserAsync(long clientUserId);
    }
}