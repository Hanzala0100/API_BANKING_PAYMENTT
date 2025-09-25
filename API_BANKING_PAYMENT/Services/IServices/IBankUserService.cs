using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IBankUserService
    {
        Task<BaseResponseDTO<ClientDTO>> CreateClientAsync(ClientCreationDTO clientDTO);
        Task<BaseResponseDTO<ClientUserCreationDTO>> CreateClientUserAsync(RegisterDTO userDTO);
        Task<BaseResponseDTO<bool>> DeleteClientAsync(long clientId);
        Task<BaseResponseDTO<bool>> DeleteClientUserAsync(long clientUserId);
        Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetAllClientsAsync();
        Task<BaseResponseDTO<IEnumerable<UserDTO>>> GetAllClientUsersByClientIdAsync(long clientId);
        Task<BaseResponseDTO<ClientDTO>> GetClientByIdAsync(long clientId);
        Task<BaseResponseDTO<UserDTO>> GetClienUserByIdAsync(long clientUserId);
        Task<BaseResponseDTO<ClientDTO>> UpdateClientAsync(ClientDTO clientDTO);
    }
}
