namespace API_BANKING_PAYMENT.Models.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public int? BankId { get; set; }
        public string? BankName { get; set; }
        public int? ClientId { get; set; }
    }
}
