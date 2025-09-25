namespace API_BANKING_PAYMENT.Models.DTO
{
    public class BankDTO
    {
        public long BankId { get; set; }
        public string BankName { get; set; }
        public string Address { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string AdminUsername { get; set; }
        public long AdminId { get; set; }
        public int TotalClients { get; set; }
        public int TotalUsers { get; set; }
    }

    public class BankCreationDTO
    {
        // Bank Details
        public string BankName { get; set; }
        public string Address { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        // Bank User Details
        public string AdminUserName { get; set; }
        public string AdminFullName { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
    }
}