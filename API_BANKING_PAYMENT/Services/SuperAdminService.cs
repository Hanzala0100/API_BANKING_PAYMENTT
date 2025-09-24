using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;

namespace API_BANKING_PAYMENT.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly IBankRepository _bankRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(
            IBankRepository bankRepository, 
            IUserRepository userRepository,
            IMapper mapper, 
            ILogger<SuperAdminService> logger,
            IConfiguration config

            ) {
            _bankRepository = bankRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _config = config;
        }

      
    }
}
