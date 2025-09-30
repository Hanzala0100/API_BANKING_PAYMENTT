using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecaptchaController : ControllerBase
    {
        private readonly IReCaptchaService _reCaptchaService;
        private readonly ILogger<RecaptchaController> _logger;

        public RecaptchaController(
            IReCaptchaService reCaptchaService,
            ILogger<RecaptchaController> logger)
        {
            _reCaptchaService = reCaptchaService;
            _logger = logger;
        }

        [HttpPost("verify")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> VerifyRecaptcha([FromBody] RecaptchaVerifyRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return BadRequest(BaseResponseDTO<bool>.ErrorResult("Token is required"));
                }

                //// TEMPORARY: Always return true for development
                //_logger.LogInformation("BYPASSING reCAPTCHA verification for development");
                //return Ok(BaseResponseDTO<bool>.SuccessResult(true, "reCAPTCHA verification successful"));

                var isValid = await _reCaptchaService.VerifyTokenAsync(request.Token);

                if (isValid)
                {
                    return Ok(BaseResponseDTO<bool>.SuccessResult(true, "reCAPTCHA verification successful"));
                }
                else
                {
                    return Ok(BaseResponseDTO<bool>.ErrorResult("reCAPTCHA verification failed"));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reCAPTCHA token");
                return StatusCode(500, BaseResponseDTO<bool>.ErrorResult("Internal server error"));
            }
        }
    }


        public class RecaptchaVerifyRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}