namespace API_BANKING_PAYMENT.Models.Settings
{
    public class ReCaptchaSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string SiteKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty; 
        public decimal? ScoreThreshold { get; set; } = 0.5m; 
    }
}
