namespace API_BANKING_PAYMENT.Models.DTO
{
    public class ReCaptchaResponse
    {
        public bool Success { get; set; }
        public string ChallengeTs { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public List<string>? ErrorCodes { get; set; }
    }
}
