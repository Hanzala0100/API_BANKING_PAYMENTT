namespace API_BANKING_PAYMENT.Models.DTO
{
    public class ReCaptchaResponse
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set;}
    }
}
