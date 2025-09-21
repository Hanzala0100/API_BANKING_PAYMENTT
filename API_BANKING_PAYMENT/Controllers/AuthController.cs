using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
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
                if (response.IsSuccess)
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
    }
}
