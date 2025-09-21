namespace API_BANKING_PAYMENT.Models.DTO
{
    public class RegisterDTO
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string Role { get; set; } = null!;

        public long? BankId { get; set; }
        public long? ClientId { get; set; }
    }
}
