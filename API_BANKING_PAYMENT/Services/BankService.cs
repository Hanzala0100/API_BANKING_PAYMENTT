using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using AutoMapper;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Services
{
    public class BankService: IBankService
    {
        private readonly IBankRepository _bankRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<SuperAdminService> _logger;
        public BankService(IConfiguration configuration, IBankRepository bankRepository, IMapper mapper, ILogger<SuperAdminService> logger)
        {
            _configuration = configuration;
            _bankRepository = bankRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BankDTO> GetBankById(int id)
        {
            var bankEntity = await _bankRepository.GetBankWithDetails(id);
            if (bankEntity == null)
                return null!;

            return _mapper.Map<BankDTO>(bankEntity);
        }

        public async Task<BankDTO> CreateBankAsync(BankDTO bankDto)
        {
            try
            {
                var bank = _mapper.Map<Bank>(bankDto);
                await _bankRepository.Add(bank);

                return _mapper.Map<BankDTO>(bank);
            }
            catch (Exception ex)
            {
         
                //   _logger.LogError(ex, "Error creating bank");

                return null!;

                // Option 2: throw custom exception
                // throw new ApplicationException("Unable to create bank", ex);
            }
        }




    }
}
