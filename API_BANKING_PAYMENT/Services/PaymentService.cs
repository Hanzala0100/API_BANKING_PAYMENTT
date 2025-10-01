using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;

namespace API_BANKING_PAYMENT.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IBeneficiaryRepository beneficiaryRepository,
            IClientRepository clientRepository,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _beneficiaryRepository = beneficiaryRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseDTO<PaymentDTO>> CreatePaymentAsync(CreatePaymentDTO paymentDTO)
        {
            try
            {
                if (paymentDTO == null)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Payment data is required");

                if (paymentDTO.Amount <= 0)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Amount must be greater than zero");

                var beneficiary = await _beneficiaryRepository.GetById(paymentDTO.BeneficiaryId);
                if (beneficiary == null || beneficiary.ClientId != paymentDTO.ClientId)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid beneficiary");

                var client = await _clientRepository.GetById(paymentDTO.ClientId);
                if (client == null)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid client");

                var payment = _mapper.Map<Payment>(paymentDTO);
                payment.Status = "Pending";
                payment.CreatedAt = DateTime.UtcNow;

                await _paymentRepository.Add(payment);

                var paymentDTOResult = _mapper.Map<PaymentDTO>(payment);
                _logger.LogInformation("Payment created successfully. PaymentId: {PaymentId}", payment.PaymentId);

                return BaseResponseDTO<PaymentDTO>.SuccessResult(paymentDTOResult, "Payment created successfully and pending approval");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for ClientId: {ClientId}", paymentDTO?.ClientId);
                return BaseResponseDTO<PaymentDTO>.ErrorResult("Failed to create payment", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<PaymentDTO>> GetPaymentByIdAsync(long paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetPaymentWithDetailsAsync(paymentId);
                if (payment == null)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Payment not found");

                var paymentDTO = _mapper.Map<PaymentDTO>(payment);
                return BaseResponseDTO<PaymentDTO>.SuccessResult(paymentDTO, "Payment retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", paymentId);
                return BaseResponseDTO<PaymentDTO>.ErrorResult("Failed to retrieve payment", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetPaymentsByClientIdAsync(long clientId)
        {
            try
            {
                var payments = await _paymentRepository.GetPaymentsByClientIdAsync(clientId);
                var paymentDTOs = _mapper.Map<IEnumerable<PaymentDTO>>(payments);

                return BaseResponseDTO<IEnumerable<PaymentDTO>>.SuccessResult(paymentDTOs, "Payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for ClientId: {ClientId}", clientId);
                return BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Failed to retrieve payments", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetAllPaymentsByBankUserId(long BankId)
        {
            try
            {
                var payments = await _paymentRepository.GetPaymentsByBankUserIdAsync(BankId);
                var paymentDTOs = _mapper.Map<IEnumerable<PaymentDTO>>(payments);

                return BaseResponseDTO<IEnumerable<PaymentDTO>>.SuccessResult(paymentDTOs, "Payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for BankId: {BankID}", BankId);
                return BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Failed to retrieve payments", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetPendingPaymentsAsync()
        {
            try
            {
                var payments = await _paymentRepository.GetPendingPaymentsAsync();
                var paymentDTOs = _mapper.Map<IEnumerable<PaymentDTO>>(payments);

                return BaseResponseDTO<IEnumerable<PaymentDTO>>.SuccessResult(paymentDTOs, "Pending payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payments");
                return BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Failed to retrieve pending payments", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<PaymentDTO>> ApprovePaymentAsync(long paymentId, long approvedBy, string notes)
        {
            try
            {
                var payment = await _paymentRepository.GetById(paymentId);
                if (payment == null)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Payment not found");

                if (payment.Status != "Pending")
                    return BaseResponseDTO<PaymentDTO>.ErrorResult($"Payment is already {payment.Status}");

                payment.Status = "Approved";
                payment.ApprovedBy = approvedBy;
                await _paymentRepository.Update(payment);

                var paymentDTO = _mapper.Map<PaymentDTO>(payment);
                _logger.LogInformation("Payment approved. PaymentId: {PaymentId}, ApprovedBy: {ApprovedBy}", paymentId, approvedBy);

                return BaseResponseDTO<PaymentDTO>.SuccessResult(paymentDTO, "Payment approved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payment with ID: {PaymentId}", paymentId);
                return BaseResponseDTO<PaymentDTO>.ErrorResult("Failed to approve payment", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<PaymentDTO>> RejectPaymentAsync(long paymentId, long rejectedBy, string notes)
        {
            try
            {
                var payment = await _paymentRepository.GetById(paymentId);
                if (payment == null)
                    return BaseResponseDTO<PaymentDTO>.ErrorResult("Payment not found");

                if (payment.Status != "Pending")
                    return BaseResponseDTO<PaymentDTO>.ErrorResult($"Payment is already {payment.Status}");

                payment.Status = "Rejected";
                payment.ApprovedBy = rejectedBy;
                await _paymentRepository.Update(payment);

                var paymentDTO = _mapper.Map<PaymentDTO>(payment);
                _logger.LogInformation("Payment rejected. PaymentId: {PaymentId}, RejectedBy: {RejectedBy}", paymentId, rejectedBy);

                return BaseResponseDTO<PaymentDTO>.SuccessResult(paymentDTO, "Payment rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting payment with ID: {PaymentId}", paymentId);
                return BaseResponseDTO<PaymentDTO>.ErrorResult("Failed to reject payment", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeletePaymentAsync(long paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetById(paymentId);
                if (payment == null)
                    return BaseResponseDTO<bool>.ErrorResult("Payment not found");

                if (payment.Status == "Approved")
                    return BaseResponseDTO<bool>.ErrorResult("Cannot delete approved payment");

                await _paymentRepository.Delete(payment);

                _logger.LogInformation("Payment deleted. PaymentId: {PaymentId}", paymentId);
                return BaseResponseDTO<bool>.SuccessResult(true, "Payment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID: {PaymentId}", paymentId);
                return BaseResponseDTO<bool>.ErrorResult("Failed to delete payment", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetPaymentsByStatusAsync(string status)
        {
            try
            {
                var validStatuses = new[] { "Pending", "Approved", "Rejected", "Processing", "Completed", "Failed" };
                if (!validStatuses.Contains(status))
                    return BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Invalid payment status");

                var payments = await _paymentRepository.GetPaymentsByStatusAsync(status);
                var paymentDTOs = _mapper.Map<IEnumerable<PaymentDTO>>(payments);

                return BaseResponseDTO<IEnumerable<PaymentDTO>>.SuccessResult(paymentDTOs, $"{status} payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments with status: {Status}", status);
                return BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Failed to retrieve payments", new List<string> { ex.Message });
            }
        }
    }
}