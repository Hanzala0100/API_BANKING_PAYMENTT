using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace API_BANKING_PAYMENT.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BeneficiaryService> _logger;

        public BeneficiaryService(
            IBeneficiaryRepository repository,
            IMapper mapper,
            ILogger<BeneficiaryService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseDTO<BeneficiaryDTO>> CreateAsync(BeneficiaryDTO model)
        {
            try
            {
                if (model == null)
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Beneficiary data is required");

                if (model.ClientId == 0)
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Client ID is required");

                if (model.AccountNumber == 0)
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Account number is required");

                if (string.IsNullOrEmpty(model.Ifsccode))
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("IFSC code is required");

                var existingBeneficiary = await _repository.GetByClientAndAccountAsync(
                    model.ClientId, model.AccountNumber, model.Ifsccode);

                if (existingBeneficiary != null)
                {
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult(
                        "Beneficiary with this account number and IFSC code already exists for this client");
                }

                var entity = _mapper.Map<Beneficiary>(model);
                await _repository.Add(entity);

                var dto = _mapper.Map<BeneficiaryDTO>(entity);

                _logger.LogInformation("Beneficiary created successfully. BeneficiaryId: {BeneficiaryId}",
                    entity.BeneficiaryId);

                return BaseResponseDTO<BeneficiaryDTO>.SuccessResult(dto, "Beneficiary created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating beneficiary for ClientId: {ClientId}", model.ClientId);
                return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Failed to create beneficiary.", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteAsync(long id)
        {
            try
            {
                var entity = await _repository.GetBeneficiaryById(id);
                if (entity == null)
                {
                    _logger.LogWarning("Beneficiary with Id: {Id} not found", id);
                    return BaseResponseDTO<bool>.ErrorResult("Beneficiary not found.");
                }

                await _repository.Delete(entity);

                _logger.LogInformation("Beneficiary with Id: {Id} deleted successfully", id);

                return BaseResponseDTO<bool>.SuccessResult(true, "Beneficiary deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting beneficiary with Id: {Id}", id);
                return BaseResponseDTO<bool>.ErrorResult("Failed to delete beneficiary.", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<List<BeneficiaryDTO>>> GetByClientIdAsync(long clientId)
        {
            try
            {
                var entities = await _repository.GetAllBeneficiariesByClientId(clientId);
                var dtoList = _mapper.Map<List<BeneficiaryDTO>>(entities);

                return BaseResponseDTO<List<BeneficiaryDTO>>.SuccessResult(dtoList, "Beneficiaries retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching beneficiaries for ClientId: {ClientId}", clientId);
                return BaseResponseDTO<List<BeneficiaryDTO>>.ErrorResult("Failed to fetch beneficiaries.", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<BeneficiaryDTO>> GetByIdAsync(long id)
        {
            try
            {
                var entity = await _repository.GetById(id);
                if (entity == null)
                {
                    _logger.LogWarning("Beneficiary with Id: {Id} not found", id);
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Beneficiary not found.");
                }

                var dto = _mapper.Map<BeneficiaryDTO>(entity);

                return BaseResponseDTO<BeneficiaryDTO>.SuccessResult(dto, "Beneficiary retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching beneficiary with Id: {Id}", id);
                return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Failed to fetch beneficiary.", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<BeneficiaryDTO>> UpdateAsync(long id, BeneficiaryDTO model)
        {
            try
            {
                var entity = await _repository.GetById(id);
                if (entity == null)
                {
                    _logger.LogWarning("Beneficiary with Id: {Id} not found", id);
                    return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Beneficiary not found.");
                }

                _mapper.Map(model, entity);

                await _repository.Update(entity);

                var dto = _mapper.Map<BeneficiaryDTO>(entity);

                _logger.LogInformation("Beneficiary with Id: {Id} updated successfully", id);

                return BaseResponseDTO<BeneficiaryDTO>.SuccessResult(dto, "Beneficiary updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating beneficiary with Id: {Id}", id);
                return BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Failed to update beneficiary.", new List<string> { ex.Message });
            }
        }
    }
}
