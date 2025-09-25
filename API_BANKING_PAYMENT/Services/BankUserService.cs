using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using AutoMapper;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services
{
    public class BankUserService: IBankUserService
    {
        private readonly IBankRepository _bankRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<SuperAdminService> _logger;
        public BankUserService(IConfiguration configuration, IBankRepository bankRepository, IMapper mapper, ILogger<SuperAdminService> logger)
        {
            _configuration = configuration;
            _bankRepository = bankRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<BaseResponseDTO<ClientDTO>> CreateClientAsync(BankCreationDTO bankCreationDTO)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<User>> CreateClientUserAsync(RegisterDTO user)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<bool>> DeleteClientAsync(long cliendId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<bool>> DeleteClientUserAsync(long clientUserId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetAllClientsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<IEnumerable<UserDTO>>> GetAllClientUsersByClientIdAsync(long clientId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<ClientDTO>> GetClientByIdAsync(long clientId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<UserDTO>> GetClienUserByIdAsync(long clientId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<ClientDTO>> UpdateClientAsync(ClientDTO clientDTO)
        {
            throw new NotImplementedException();
        }
    }
}
