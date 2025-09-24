using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IEmployeeService _service;
        public ClientController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        [Authorize(Roles = "ClientUser")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");

            var response = await _service.RegisterAsync(model);

            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }



    }
}
