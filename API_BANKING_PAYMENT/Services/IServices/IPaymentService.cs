using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IPaymentService
    {
        Task<BaseResponseDTO<PaymentDTO>> CreatePaymentAsync(CreatePaymentDTO paymentDTO);
        Task<BaseResponseDTO<PaymentDTO>> GetPaymentByIdAsync(long paymentId);
        Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetPaymentsByClientIdAsync(long clientId);
        Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetPendingPaymentsAsync();
        Task<BaseResponseDTO<PaymentDTO>> ApprovePaymentAsync(long paymentId, long approvedBy, string notes);
        Task<BaseResponseDTO<PaymentDTO>> RejectPaymentAsync(long paymentId, long rejectedBy, string notes);
        Task<BaseResponseDTO<bool>> DeletePaymentAsync(long paymentId);
        Task<BaseResponseDTO<IEnumerable<PaymentDTO>>> GetPaymentsByStatusAsync(string status);
    }
}
