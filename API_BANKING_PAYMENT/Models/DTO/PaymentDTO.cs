namespace API_BANKING_PAYMENT.Models.DTO
{
    public class PaymentDTO
    {
        public long PaymentId { get; set; }
        public long? ClientId { get; set; }
        public string? ClientName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = null!;
        public long? ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? BeneficiaryId { get; set; }
        public string? BeneficiaryName { get; set; }

    }
}
