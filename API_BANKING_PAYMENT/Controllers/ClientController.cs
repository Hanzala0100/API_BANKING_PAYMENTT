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
       // [Authorize(Roles = "ClientUser")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");

            var response = await _service.CreateAsync(model);

            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("{id}")]
       // [Authorize(Roles = "ClientUser")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");
            var isUpdated = await _service.UpdateAsync(id, model);
            if (!isUpdated.Success)
                return NotFound(new { Message = "Employee not found." });
            return NoContent();
        }

        [HttpDelete("{id}")]
       // [Authorize(Roles = "ClientUser")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var isDeleted = await _service.DeleteAsync(id);
            if (!isDeleted.Success)
                return NotFound(new { Message = "Employee not found." });
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _service.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { Message = "Employee not found." });
            return Ok(employee);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _service.GetAllAsync();
            return Ok(employees);
        }



    }
}
