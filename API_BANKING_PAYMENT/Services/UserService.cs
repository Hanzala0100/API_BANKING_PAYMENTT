using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_BANKING_PAYMENT.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public UserService(IUserRepository repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _mapper = mapper;
        }
        public async Task<LoginResponseModel> LoginAsync(LoginViewModel user)
        {
            var existingUser = await _repository.GetByEmailAsync(user.Email);
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
                        new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                        new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                    };

            var expiry = DateTime.UtcNow.AddMinutes(10);

            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: signingCredentials);

            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(tokenOptions),
                Expiry = expiry
            };
        }

    }
}
