using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Services;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _service;
        public AuthController(IUserService service)
        {
            _service = service;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel user)
        {
            LoginResponseModel response;
            if (ModelState.IsValid)
            {
                response = await _service.LoginAsync(user);
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return Unauthorized(response);
                }
            }
            return BadRequest();
        }

        // login with ReCaptcha 
        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginViewModel user, [FromServices] RecaptchaService recaptchaService)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest();

        //    // Verify reCAPTCHA first
        //    bool isHuman = await recaptchaService.VerifyTokenAsync(user.RecaptchaToken);
        //    if (!isHuman)
        //        return BadRequest(new { Message = "reCAPTCHA validation failed" });

        //    // Proceed with login
        //    var response = await _service.LoginAsync(user);

        //    if (response.IsSuccess)
        //        return Ok(response);
        //    else
        //        return Unauthorized(response);
        //}



        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest(new { Message = "No User Logged In" });


            return Ok(new { Message = "Logged out successfully." });
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest(new { Message = "No User Logged In" });


            return Ok(new { Message = "Logged out successfully." });
        }

        // [HttpPost("register")]
        // public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest("Invalid request.");

        //     var response = await _service.RegisterAsync(model);

        //     if (!response.IsSuccess)
        //         return BadRequest(response);

        //     return Ok(response);
        // }
    }
}
