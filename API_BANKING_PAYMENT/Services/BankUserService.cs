using API_BANKING_PAYMENT.Models.Enum;
using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API_BANKING_PAYMENT.Services
{
    public class BankUserService : IBankUserService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDocumentService _documentService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<BankUserService> _logger;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public BankUserService(
            IConfiguration configuration,
            IClientRepository clientRepository,
            IUserRepository userRepository,
            IDocumentService documentService,
            IMapper mapper,
            ILogger<BankUserService> logger,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _configuration = configuration;
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _documentService = documentService;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<BaseResponseDTO<ClientDTO>> CreateClientAsync(ClientCreationDTO clientDTO)
        {
            try
            {
                if (clientDTO == null)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Client data cannot be null");

                if (string.IsNullOrEmpty(clientDTO.RegisterationNumber))
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Registration number is required");

                if (clientDTO.BankId <= 0)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Valid Bank ID is required");

                var existingClient = await _clientRepository.GetClientByRegisterationNumber(clientDTO.RegisterationNumber);
                if (existingClient != null)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Client with same registration number already exists");

                var clientModel = new Client
                {
                    ClientName = clientDTO.ClientName,
                    RegisterationNumber = clientDTO.RegisterationNumber,
                    BankId = clientDTO.BankId,
                    Address = clientDTO.Address ?? string.Empty,
                    VerificationStatus = VerificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                var createdClient = await _clientRepository.AddClientAsync(clientModel);

                var clientDto = new ClientDTO
                {
                    ClientId = createdClient.ClientId,
                    ClientName = createdClient.ClientName,
                    RegisterationNumber = createdClient.RegisterationNumber,
                    Address = createdClient.Address,
                    VerificationStatus = createdClient.VerificationStatus,
                    BankId = clientDTO.BankId,
                    BankName = clientDTO.BankName,
                    TotalEmployees = 0,
                    TotalBeneficiaries = 0,
                    TotalPayments = 0
                };

                return BaseResponseDTO<ClientDTO>.SuccessResult(clientDto, $"Client added successfully. Verification status: {VerificationStatus.Pending}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating client with registration number: {RegNumber}", clientDTO?.RegisterationNumber ?? "Unknown");
                return BaseResponseDTO<ClientDTO>.ErrorResult("Error occurred while creating client", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<DocumentDTO>> UploadClientDocumentAsync(long clientId, IFormFile file, long uploadedBy, long bankId, string docType)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Client not found");

                if (file == null || file.Length == 0)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("File is required");

                //var validDocTypes = new[] { "BusinessLicense", "KYCDocument", "TaxCertificate", "BankStatement", "AddressProof", "IdentityProof" };
                //if (!validDocTypes.Contains(docType))
                //    return BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid document type");

                var result = await _documentService.UploadDocumentAsync(file, uploadedBy, bankId, clientId, docType);

                if (result.Success)
                {
                    _logger.LogInformation("Document uploaded successfully for client ID: {ClientId}, Document Type: {DocType}", clientId, docType);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading document for client ID: {ClientId}", clientId);
                return BaseResponseDTO<DocumentDTO>.ErrorResult("Error occurred while uploading document", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<ClientDTO>> VerifyClientAsync(long clientId, long verifiedBy, long bankId, string verificationStatus, string notes)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Client not found");

                if (client.BankId != bankId)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("You can only verify clients from your own bank");

                if (!VerificationStatus.GetAllStatuses().Contains(verificationStatus))
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Invalid verification status");

                var oldStatus = client.VerificationStatus;

                if (!VerificationStatus.IsValidTransition(oldStatus, verificationStatus))
                    return BaseResponseDTO<ClientDTO>.ErrorResult($"Invalid status transition from {oldStatus} to {verificationStatus}");

                client.VerificationStatus = verificationStatus;
                client.VerifiedBy = verifiedBy;
                client.VerifiedAt = DateTime.UtcNow;
                client.VerificationNotes = notes;

                var result = await _clientRepository.Update(client);

                if (result)
                {
                    // Send email only for approved or rejected status
                    if (verificationStatus.ToLower() == "approved" || verificationStatus.ToLower() == "verified")
                    {
                        await _emailService.SendApprovalEmailAsync(clientId, notes);
                    }
                    else if (verificationStatus.ToLower() == "rejected")
                    {
                        await _emailService.SendRejectionEmailAsync(clientId, notes);
                    }

                    var clientDTO = _mapper.Map<ClientDTO>(client);
                    _logger.LogInformation("Client verification status updated: Client ID: {ClientId}, From: {OldStatus}, To: {NewStatus}",
                        clientId, oldStatus, verificationStatus);

                    return BaseResponseDTO<ClientDTO>.SuccessResult(clientDTO, $"Client verification status updated to {verificationStatus}");
                }
                else
                {
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Failed to update client verification status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying client with ID: {ClientId}", clientId);
                return BaseResponseDTO<ClientDTO>.ErrorResult("Error occurred while verifying client", new List<string> { ex.Message });
            }
        }
        public async Task<BaseResponseDTO<IEnumerable<DocumentDTO>>> GetClientDocumentsAsync(long clientId)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Client not found");

                var documents = await _clientRepository.GetClientDocumentsAsync(clientId);
                var documentDTOs = _mapper.Map<IEnumerable<DocumentDTO>>(documents);

                return BaseResponseDTO<IEnumerable<DocumentDTO>>.SuccessResult(documentDTOs, "Client documents retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving documents for client ID: {ClientId}", clientId);
                return BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Error occurred while retrieving client documents", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetClientsByVerificationStatusAsync(string verificationStatus)
        {
            try
            {
                if (!VerificationStatus.GetAllStatuses().Contains(verificationStatus))
                    return BaseResponseDTO<IEnumerable<ClientDTO>>.ErrorResult("Invalid verification status");

                var clients = await _clientRepository.GetClientsByVerificationStatusAsync(verificationStatus);
                var clientDTOs = _mapper.Map<IEnumerable<ClientDTO>>(clients);

                return BaseResponseDTO<IEnumerable<ClientDTO>>.SuccessResult(clientDTOs, $"Clients with status '{verificationStatus}' retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving clients with status: {Status}", verificationStatus);
                return BaseResponseDTO<IEnumerable<ClientDTO>>.ErrorResult("Error occurred while retrieving clients", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetClientsWithPendingVerificationAsync()
        {
            try
            {
                var clients = await _clientRepository.GetClientsWithPendingVerificationAsync();
                var clientDTOs = _mapper.Map<IEnumerable<ClientDTO>>(clients);

                return BaseResponseDTO<IEnumerable<ClientDTO>>.SuccessResult(clientDTOs, "Clients with pending verification retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving clients with pending verification");
                return BaseResponseDTO<IEnumerable<ClientDTO>>.ErrorResult("Error occurred while retrieving clients", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<ClientUserCreationDTO>> CreateClientUserAsync(RegisterDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                    return BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("User data cannot be null");

                Client? client = null;
                if (userDTO.ClientId.HasValue)
                {
                    client = await _clientRepository.GetById(userDTO.ClientId.Value);
                    if (client == null)
                        return BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("Client not found");

                    //if (client.VerificationStatus != VerificationStatus.Verified)
                    //    return BaseResponseDTO<ClientUserCreationDTO>.ErrorResult(
                    //        $"Cannot create user for client with status: {client.VerificationStatus}. Client must be {VerificationStatus.Verified}.");
                }

                var existingUser = await _userRepository.GetByEmailAsync(userDTO.Email) ?? await _userRepository.GetByUsernameAsync(userDTO.UserName);
                if (existingUser != null)
                    return BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("User with same email or username already exists");

                var userModel = _mapper.Map<User>(userDTO);
                userModel.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
                userModel.Role = "ClientUser";
                userModel.CreatedAt = DateTime.UtcNow;

                var createdUser = await _userRepository.AddClientUser(userModel);

                var clientUserDto = new ClientUserCreationDTO
                {
                    UserId = createdUser.UserId,
                    UserName = createdUser.UserName,
                    FullName = createdUser.FullName,
                    Password = userDTO.Password,
                    Email = createdUser.Email,
                    Role = createdUser.Role,
                    ClientId = (int?)createdUser.ClientId
                };

                await _emailService.SendClientUserWelcomeEmailAsync(clientUserDto);

                if (client != null && client.VerificationStatus == VerificationStatus.Pending)
                {
                    await _emailService.SendPendingVerificationEmailAsync(client.ClientId, clientUserDto.Email);
                }

                return BaseResponseDTO<ClientUserCreationDTO>.SuccessResult(clientUserDto, "Client user created successfully and emails sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating client user with email: {Email}", userDTO?.Email ?? "Unknown");
                return BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("Error occurred while creating client user", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteClientAsync(long clientId)
        {
            try
            {
                var existingClient = await _clientRepository.GetById(clientId);
                if (existingClient == null)
                    return BaseResponseDTO<bool>.ErrorResult("Client not found");

                var result = await _clientRepository.Delete(existingClient);
                if (result)
                    return BaseResponseDTO<bool>.SuccessResult(true, "Client deleted successfully");
                else
                    return BaseResponseDTO<bool>.ErrorResult("Failed to delete client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting client with ID: {ClientId}", clientId);
                return BaseResponseDTO<bool>.ErrorResult("Error occurred while deleting client", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteClientUserAsync(long clientUserId)
        {
            try
            {
                var existingUser = await _userRepository.GetClientById(clientUserId);
                if (existingUser == null)
                    return BaseResponseDTO<bool>.ErrorResult("Client user not found");

                if (existingUser.Role != "ClientUser")
                    return BaseResponseDTO<bool>.ErrorResult("User is not a client user");

                var result = await _userRepository.Delete(existingUser);
                if (result)
                    return BaseResponseDTO<bool>.SuccessResult(true, "Client user deleted successfully");
                else
                    return BaseResponseDTO<bool>.ErrorResult("Failed to delete client user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting client user with ID: {UserId}", clientUserId);
                return BaseResponseDTO<bool>.ErrorResult("Error occurred while deleting client user", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<ClientDTO>>> GetAllClientsAsync(long id)
        {
            try
            {
                var clients = await _clientRepository.GetClientsAllAsync(id);  
                var clientDTOs = _mapper.Map<IEnumerable<ClientDTO>>(clients);
                return BaseResponseDTO<IEnumerable<ClientDTO>>.SuccessResult(clientDTOs, "Clients retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all clients");
                return BaseResponseDTO<IEnumerable<ClientDTO>>.ErrorResult("Error occurred while retrieving clients", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<UserDTO>>> GetAllClientUsersByClientIdAsync(long clientId)
        {
            try
            {
                var users = await _userRepository.GetUsersByClientId(clientId);
                var clientUsers = users.Where(u => u.Role == "ClientUser");
                var userDTOs = _mapper.Map<IEnumerable<UserDTO>>(clientUsers);
                return BaseResponseDTO<IEnumerable<UserDTO>>.SuccessResult(userDTOs, "Client users retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving client users for client ID: {ClientId}", clientId);
                return BaseResponseDTO<IEnumerable<UserDTO>>.ErrorResult("Error occurred while retrieving client users", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<ClientDTO>> GetClientByIdAsync(long clientId)
        {
            try
            {
                var client = await _clientRepository.GetClientByIdAsync(clientId);
                if (client == null)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Client not found");

                var clientDTO = _mapper.Map<ClientDTO>(client);
                return BaseResponseDTO<ClientDTO>.SuccessResult(clientDTO, "Client retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving client with ID: {ClientId}", clientId);
                return BaseResponseDTO<ClientDTO>.ErrorResult("Error occurred while retrieving client", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<UserDTO>> GetClienUserByIdAsync(long clientUserId)
        {
            try
            {
                var user = await _userRepository.GetById(clientUserId);
                if (user == null)
                    return BaseResponseDTO<UserDTO>.ErrorResult("Client user not found");

                if (user.Role != "ClientUser")
                    return BaseResponseDTO<UserDTO>.ErrorResult("User is not a client user");

                var userDTO = _mapper.Map<UserDTO>(user);
                return BaseResponseDTO<UserDTO>.SuccessResult(userDTO, "Client user retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving client user with ID: {UserId}", clientUserId);
                return BaseResponseDTO<UserDTO>.ErrorResult("Error occurred while retrieving client user", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<ClientDTO>> UpdateClientAsync(ClientDTO clientDTO)
        {
            try
            {
                var existingClient = await _clientRepository.GetById(clientDTO.ClientId);
                if (existingClient == null)
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Client not found");

                _mapper.Map(clientDTO, existingClient);
                var result = await _clientRepository.Update(existingClient);

                if (result)
                {
                    var updatedDTO = _mapper.Map<ClientDTO>(existingClient);
                    return BaseResponseDTO<ClientDTO>.SuccessResult(updatedDTO, "Client updated successfully");
                }
                else
                {
                    return BaseResponseDTO<ClientDTO>.ErrorResult("Failed to update client");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating client with ID: {ClientId}", clientDTO.ClientId);
                return BaseResponseDTO<ClientDTO>.ErrorResult("Error occurred while updating client", new List<string> { ex.Message });
            }
        }

    }
}