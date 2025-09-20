namespace API_BANKING_PAYMENT.Models.DTO
{
    public class ClientDTO
    {
        public long ClientId { get; set; }
        public string ClientName { get; set; }
        public string RegisterationNumber { get; set; }
        public string VerificationStatus { get; set; }
        public string BankName { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalBeneficiaries { get; set; }
        public int TotalPayments { get; set; }
    }
}
