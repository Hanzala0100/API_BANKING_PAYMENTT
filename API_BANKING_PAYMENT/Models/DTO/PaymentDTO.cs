namespace API_BANKING_PAYMENT.Models.DTO
{
    public class PaymentDTO
    {
        public long PaymentId { get; set; }
        public long? ClientId { get; set; }
        public long? BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = "Pending";
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; }
    }

    public class CreatePaymentDTO
    {
        public long ClientId { get; set; }
        public long BeneficiaryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }

    public class ApprovePaymentDTO
    {
        public bool IsApproved { get; set; }
        public string Notes { get; set; }
    }

    public class PaymentResponseDTO
    {
        public long PaymentId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string TransactionReference { get; set; }
    }

    public class ApprovePaymentRequestDTO
    {
        public string Notes { get; set; }
    }
}