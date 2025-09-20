namespace API_BANKING_PAYMENT.Models.DTO
{
    public class BankDTO
    {
        public long BankId { get; set; }
        public string BankName { get; set; }
        public string Address { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public int TotalClients { get; set; }
        public int TotalUsers { get; set; }
    }
}
