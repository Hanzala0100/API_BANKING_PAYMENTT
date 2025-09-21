namespace API_BANKING_PAYMENT.Models.DTO
{
    public class TokenDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}
