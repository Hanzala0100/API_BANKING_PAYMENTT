using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using AutoMapper;

namespace API_BANKING_PAYMENT.Services
{
    public class BankService: IBankService
    {
        private readonly IConfiguration _configuration;
        private readonly IBankRepository _bankRepository;
        private readonly IMapper _mapper;
        public BankService(IConfiguration configuration, IBankRepository bankRepository, IMapper mapper)
        {
            _configuration = configuration;
            _bankRepository = bankRepository;
            _mapper = mapper;
        }

        public async Task<BankDTO> GetBankById(int id)
        {
            var bankEntity = await _bankRepository.GetBankWithDetails(id);
            if (bankEntity == null)
                return null!;

            return _mapper.Map<BankDTO>(bankEntity);
        }


    }
}
