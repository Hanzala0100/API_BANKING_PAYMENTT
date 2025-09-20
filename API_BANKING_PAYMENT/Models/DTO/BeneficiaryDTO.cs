using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Models.DTO
{
    public class BeneficiaryDTO
    {
        public long BeneficiaryId { get; set; }
        public long ClientId { get; set; }
        public string FullName { get; set; } = null!;
        public long AccountNumber { get; set; }
        public string BankName { get; set; } = null!;
        public string Ifsccode { get; set; } = null!;
        public int TotalPayments { get; set; }
    }
}
