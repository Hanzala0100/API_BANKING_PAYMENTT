namespace API_BANKING_PAYMENT.Models.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? BankId { get; set; }
        public string? BankName { get; set; }
        public int? ClientId { get; set; }
    }
}
