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

        public ReCaptchaService(IHttpClientFactory httpClientFactory, IOptions<ReCaptchaSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

       public async Task<bool> VerifyTokenAsync(string token)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}"
                );

            var reCaptchaResponse = System.Text.Json.JsonSerializer.Deserialize<ReCaptchaResponse>(response);

            return reCaptchaResponse != null && reCaptchaResponse.Success;
        }
    }
}
