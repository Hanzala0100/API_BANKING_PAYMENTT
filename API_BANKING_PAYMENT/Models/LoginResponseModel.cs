namespace API_BANKING_PAYMENT.Models
{
    public class LoginResponseModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int? BankId { get; set; }
        public string? BankName { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }

        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
    }
}
