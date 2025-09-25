using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IBankUserService
    {

        //Clients
        Task<BaseResponseDTO<ClientDTO>> CreateClientAsync(BankCreationDTO bankCreationDTO);
        Task<BaseResponseDTO<ClientDTO>> UpdateClientAsync(ClientDTO clientDTO);
        Task<BaseResponseDTO<bool>> DeleteClientAsync(long clientId);
        Task<BaseResponseDTO<ClientDTO>> GetClientByIdAsync(long clientId);
        Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetAllClientsAsync();

        //Client User 
        Task<BaseResponseDTO<User>> CreateClientUserAsync(RegisterDTO user);
        Task<BaseResponseDTO<bool>> DeleteClientUserAsync(long clientUserId);
        Task<BaseResponseDTO<UserDTO>> GetClienUserByIdAsync(long clientId);
        Task<BaseResponseDTO<IEnumerable<UserDTO>>> GetAllClientUsersByClientIdAsync(long clientId);
    }
}
