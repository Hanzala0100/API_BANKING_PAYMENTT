using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Enum;
using API_BANKING_PAYMENT.Respositories;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using CloudinaryDotNet.Actions;

namespace API_BANKING_PAYMENT.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly IBankRepository _bankRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(
            IBankRepository bankRepository, 
            IUserRepository userRepository,
            IClientRepository clientRepository,
            IMapper mapper, 
            ILogger<SuperAdminService> logger,
            IConfiguration config

            ) {
            _bankRepository = bankRepository;
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _logger = logger;
            _config = config;
        }

        public async Task<BaseResponseDTO<BankDTO>> CreateBankAsync(BankCreationDTO bankCreationDTO)
        {
            try
            {
                var existingBank = await _bankRepository.GetBankByName(bankCreationDTO.BankName);
                if (existingBank != null)
                    return BaseResponseDTO<BankDTO>.ErrorResult("Bank with this name already exists");

                var existingUser = await _userRepository.GetByEmailAsync(bankCreationDTO.AdminEmail)
                                 ?? await _userRepository.GetByUsernameAsync(bankCreationDTO.AdminUserName);
                if (existingUser != null)
                    return BaseResponseDTO<BankDTO>.ErrorResult("Admin user with this email/username already exists");

                var bank = _mapper.Map<Bank>(bankCreationDTO);
                bank.CreatedAt = DateTime.UtcNow;

                await _bankRepository.Add(bank);

                var bankAdmin = new User
                {
                    UserName = bankCreationDTO.AdminUserName,
                    FullName = bankCreationDTO.AdminFullName,
                    Email = bankCreationDTO.AdminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(bankCreationDTO.AdminPassword),
                    Role = "BankUser",
                    BankId = bank.BankId,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.Add(bankAdmin);

                var bankDTO = _mapper.Map<BankDTO>(bank);
                bankDTO.AdminUsername = bankAdmin.UserName;  
                bankDTO.AdminId = bankAdmin.UserId;          

                return BaseResponseDTO<BankDTO>.SuccessResult(bankDTO,
                    "Bank and admin user created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bank with admin user");
                return BaseResponseDTO<BankDTO>.ErrorResult("Error creating bank",
                    new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteBankAsync(long bankId)
        {
            try
            {
                var existingBank = await _bankRepository.GetById(bankId);
                if (existingBank == null)
                {
                    _logger.LogWarning("Delete attempt failed. Bank with ID {BankId} not found.", bankId);
                    return BaseResponseDTO<bool>.ErrorResult("Bank not found");
                }

                var users = await _userRepository.GetUsersByBankId(bankId);
                foreach (var user in users)
                {
                    user.BankId = null;
                    user.ClientId = null; 
                    await _userRepository.Update(user);
                }

                 
                var clients = await _clientRepository.GetClientsByBankId(bankId);
                foreach (var client in clients)
                {
                    client.BankId = null;
                    await _clientRepository.Update(client);
                }

               
                var result = await _bankRepository.Delete(existingBank);

                if (!result)
                {
                    _logger.LogError("Delete operation failed for Bank with ID {BankId}", bankId);
                    return BaseResponseDTO<bool>.ErrorResult("Failed to delete bank");
                }

                _logger.LogInformation("Bank with ID {BankId} deleted successfully.", bankId);
                return BaseResponseDTO<bool>.SuccessResult(true, "Bank deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting Bank with ID {BankId}", bankId);
                return BaseResponseDTO<bool>.ErrorResult("An error occurred while deleting the bank.");
            }
        }


        public async Task<BaseResponseDTO<IEnumerable<BankDTO>>> GetAllBanksAsync()
        {
            try
            {
                var banks = await _bankRepository.GetAll();

                if (banks == null || !banks.Any())
                {
                    return BaseResponseDTO<IEnumerable<BankDTO>>.SuccessResult(
                        Enumerable.Empty<BankDTO>(),
                        "No banks found"
                    );
                }

                var bankDTOs = banks.Select(bank =>
                {
                    var dto = _mapper.Map<BankDTO>(bank);

                    var adminUser = bank.Users?.FirstOrDefault(u => u.Role == "BankUser");
                    if (adminUser != null)
                    {
                        dto.AdminUsername = adminUser.UserName;
                        dto.AdminId = adminUser.UserId;
                    }

                    dto.TotalClients = bank.Clients?.Count ?? 0;
                    dto.TotalUsers = bank.Users?.Count ?? 0;

                    return dto;
                });

                return BaseResponseDTO<IEnumerable<BankDTO>>.SuccessResult(
                    bankDTOs,
                    "Banks fetched successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all banks");
                return BaseResponseDTO<IEnumerable<BankDTO>>.ErrorResult(
                    "Error fetching banks",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<BaseResponseDTO<BankDTO>> GetBankByIdAsync(long bankId)
        {
            try
            {
                var bank = await _bankRepository.GetById(bankId);
                if (bank == null)
                {
                    _logger.LogWarning("Bank not found with ID: {BankId}", bankId);
                    return BaseResponseDTO<BankDTO>.ErrorResult("Bank not found");
                }

                var dto = _mapper.Map<BankDTO>(bank);

                var adminUser = bank.Users?.FirstOrDefault(u => u.Role == "BankUser");
                if (adminUser != null)
                {
                    dto.AdminUsername = adminUser.UserName;
                    dto.AdminId = adminUser.UserId;
                }

                dto.TotalClients = bank.Clients?.Count ?? 0;
                dto.TotalUsers = bank.Users?.Count ?? 0;

                _logger.LogInformation("Bank fetched successfully: {BankId} - {BankName}", bankId, bank.BankName);
                return BaseResponseDTO<BankDTO>.SuccessResult(dto, "Bank fetched successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bank with ID: {BankId}", bankId);
                return BaseResponseDTO<BankDTO>.ErrorResult(
                    "Error fetching bank details",
                    new List<string> { ex.Message }
                );
            }
        }


        public Task<BaseResponseDTO<ReportDTO>> GenerateAuditLogReportAsync(ReportRequestDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDTO<ReportDTO>> GenerateSystemUsageReportAsync(ReportRequestDTO request)
        {
            throw new NotImplementedException();
        }

    

        public Task<BaseResponseDTO<BankDTO>> UpdateBankAsync(BankDTO bankDTO)
        {
            throw new NotImplementedException();
        }
    }
}
;