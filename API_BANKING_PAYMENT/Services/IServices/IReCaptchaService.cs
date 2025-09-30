namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IReCaptchaService
    {
        Task<bool> VerifyTokenAsync(string token);
    }
}
