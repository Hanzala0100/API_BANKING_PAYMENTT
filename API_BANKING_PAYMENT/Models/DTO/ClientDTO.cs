namespace API_BANKING_PAYMENT.Models.DTO
{

    public class ClientCreationDTO
    {
        public string ClientName { get; set; }
        public string RegisterationNumber { get; set; }
        public string VerificationStatus { get; set; }
        public string Address { get; set; } = null!;
        public long BankId { get; set; }
        public string BankName { get; set; }
    }
    public class ClientDTO
    {
        public long ClientId { get; set; }
        public string ClientName { get; set; }
        public string RegisterationNumber { get; set; }
        public string Address { get; set; } = null!;
        public string VerificationStatus { get; set; }
        public long BankId { get; set; }
        public string BankName { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalBeneficiaries { get; set; }
        public int TotalPayments { get; set; }
    }

    public class  ClientUserCreationDTO 
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? ClientId { get; set; }
    }
}
