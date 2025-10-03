using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Settings;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.Extensions.Options;

namespace API_BANKING_PAYMENT.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReCaptchaSettings _settings;
        private readonly ILogger<ReCaptchaService> _logger;

        public ReCaptchaService(
            IHttpClientFactory httpClientFactory,
            IOptions<ReCaptchaSettings> settings,
            ILogger<ReCaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
            
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Using reCAPTCHA secret key: {SecretKey}", _settings.SecretKey);

                var client = _httpClientFactory.CreateClient();
                var url = $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}";

                _logger.LogInformation("Calling reCAPTCHA API: {Url}", url);

                var response = await client.GetStringAsync(url);
                var reCaptchaResponse = System.Text.Json.JsonSerializer.Deserialize<ReCaptchaResponse>(response);

                _logger.LogInformation("ReCAPTCHA verification response: {Response}",
                    System.Text.Json.JsonSerializer.Serialize(reCaptchaResponse));

                return reCaptchaResponse?.Success ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reCAPTCHA token");
                return false;
            }
        }
    }
}