using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using System;
using System.Threading.Tasks;

namespace API_BANKING_PAYMENT.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IBankService _bankService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IBankService bankService, IMapper mapper)
        {
            _repository = repository;
            _bankService = bankService;
            _mapper = mapper;
        }

        //public async Task<RegisterResponseModel> RegisterAsync(RegisterDTO model)
        //{
        //    // Check if user already exists
        //    var existingUser = await _repository.GetByEmailAsync(model.Email);
        //    if (existingUser != null)
        //    {
        //        return new RegisterResponseModel
        //        {
        //            IsSuccess = false,
        //            Message = "User already exists with this email."
        //        };
        //    }

        //    // Map DTO to User entity
        //    var user = _mapper.Map<User>(model);
        //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        //    user.CreatedAt = DateTime.UtcNow;

        //    if (user.Role == "BankUser" || user.Role == "ClientUser")
        //    {
        //        if (!model.BankId.HasValue)
        //        {
        //            return new RegisterResponseModel
        //            {
        //                IsSuccess = false,
        //                Message = "BankId is required for this role."
        //            };
        //        }

        //        var bank = await _bankService.GetBankById((int)model.BankId);
        //        if (bank != null)
        //        {
        //            user.BankId = bank.BankId;
        //            //user.Bank.BankName = bank.BankName;  
        //        }

        //        // Extra steps for ClientUser
        //        if (user.Role == "ClientUser")
        //        {
        //            var client = new Client
        //            {
        //                BankId = model.BankId.Value,
        //                ClientName = user.FullName,
        //                RegisterationNumber = Guid.NewGuid().ToString(),
        //                Address = "N/A",
        //                VerificationStatus = "Pending",
        //                CreatedAt = DateTime.UtcNow
        //            };

        //            await _repository.AddClientAsync(client);
        //            user.ClientId = client.ClientId;
        //        }
        //    }


        //    await _repository.Add(user);

        //    return new RegisterResponseModel
        //    {
        //        IsSuccess = true,
        //        Message = "User registered successfully."
        //    };
        //}

        public async Task<LoginResponseModel> LoginAsync(LoginViewModel model)
        {
            var existingUser = await _repository.GetByEmailAsync(model.Email);

            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(model.Password, existingUser.PasswordHash))
            {
                return new LoginResponseModel
                {
                    IsSuccess = false,
                    Message = "Invalid email or password."
                };
            }

            var userDto = _mapper.Map<UserDTO>(existingUser);

            return new LoginResponseModel
            {
                User = userDto,
                IsSuccess = true,
                Message = "Login successful."
            };
        }

    }
}
