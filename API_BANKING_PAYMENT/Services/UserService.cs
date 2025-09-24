using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API_BANKING_PAYMENT.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IBankService _bankService;
        private readonly IMapper _mapper;
        private readonly ILogger<SuperAdminService> _logger;
        private readonly IConfiguration _config;

        public UserService(IUserRepository repository, IBankService bankService, IMapper mapper, ILogger<SuperAdminService> logger, IConfiguration config)
        {
            _repository = repository;
            _bankService = bankService;
            _mapper = mapper;
            _logger = logger;
            _config = config;
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

        public async Task<LoginResponseModel> LoginAsync(LoginViewModel user)
        {
            var existingUser = await _repository.GetByUsernameAsync(user.Username);
            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, existingUser.PasswordHash))
            {
                return new LoginResponseModel
                {
                    IsSuccess = false,
                    Message = "Invalid email or password."
                };
            }
 
            return new LoginResponseModel
            {
                User = _mapper.Map<UserDTO>(existingUser),
                IsSuccess = true,
                Token = GenerateJWTToken(existingUser),
                Message = "Login successful.",
            };
        }

        private TokenDTO GenerateJWTToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>

                    {

                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        //user name 
                        new Claim(ClaimTypes.UserData,user.UserName?? string.Empty),
                        new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                        new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                    };

            var expiry = DateTime.UtcNow.AddDays(10);
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: signingCredentials);
            return new TokenDTO
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions),
                Expiry = expiry
            };
        }
    }

}
 