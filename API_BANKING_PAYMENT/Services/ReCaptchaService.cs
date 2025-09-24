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
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}"
                );

                var reCaptchaResponse = System.Text.Json.JsonSerializer.Deserialize<ReCaptchaResponse>(response);

                if (reCaptchaResponse == null)
                {
                    _logger.LogWarning("ReCAPTCHA verification returned null response for token: {Token}", token);
                    return false;
                }

                if (reCaptchaResponse.Success)
                {
                    _logger.LogInformation("ReCAPTCHA verification succeeded for token: {Token}", token);
                }
                else
                {
                    _logger.LogWarning("ReCAPTCHA verification failed. Token: {Token}, Errors: {Errors}",
                        token, string.Join(", ", reCaptchaResponse.ErrorCodes ?? new List<string>()));
                }

                return reCaptchaResponse.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying ReCAPTCHA token: {Token}", token);
                throw;
            }
        }
    }
}
