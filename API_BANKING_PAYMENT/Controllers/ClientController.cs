using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ClientUser")]
    public class ClientController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IBankUserService _bankUserService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IEmployeeService employeeService, IBankUserService bankUserService, ILogger<ClientController> logger)
        {
            _employeeService = employeeService;
            _bankUserService = bankUserService;
            _logger = logger;
        }

        // CLIENT USER MANAGEMENT 

        [HttpPost("users")]
        public async Task<ActionResult<BaseResponseDTO<ClientUserCreationDTO>>> CreateClientUser([FromBody] RegisterDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("Invalid input data"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("Invalid client association"));
            }

            userDTO.ClientId = currentClientId;

            var result = await _bankUserService.CreateClientUserAsync(userDTO);

            if (result.Success)
            {
                _logger.LogInformation("Client user created successfully by client user for client ID: {ClientId}", currentClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client user creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<UserDTO>>>> GetClientUsers()
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<UserDTO>>.ErrorResult("Invalid client association"));
            }

            var result = await _bankUserService.GetAllClientUsersByClientIdAsync(currentClientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<BaseResponseDTO<UserDTO>>> GetClientUserById(long userId)
        {
            if (userId <= 0)
            {
                return BadRequest(BaseResponseDTO<UserDTO>.ErrorResult("Invalid user ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");

            var result = await _bankUserService.GetClienUserByIdAsync(userId);

            if (result.Success && result.Data?.ClientId != currentClientId)
            {
                return Forbid(); // User doesn't have access to this user
            }

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        //[HttpDelete("users/{userId}")]
        //public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteClientUser(long userId)
        //{
        //    if (userId <= 0)
        //    {
        //        return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid user ID"));
        //    }

        //    // Get current user's client ID from claims
        //    var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");

        //    // First check if the user belongs to the same client
        //    var userResult = await _bankUserService.GetClienUserByIdAsync(userId);
        //    if (!userResult.Success || userResult.Data?.ClientId != currentClientId)
        //    {
        //        return Forbid(); // User doesn't have permission to delete this user
        //    }

        //    var result = await _bankUserService.DeleteClientUserAsync(userId);

        //    if (result.Success)
        //    {
        //        _logger.LogInformation("Client user deleted successfully: {UserId}", userId);
        //        return Ok(result);
        //    }
        //    else
        //    {
        //        _logger.LogWarning("Client user deletion failed: {Message}", result.Message);
        //        return BadRequest(result);
        //    }
        //}

        // EMPLOYEE MANAGEMENT  

        [HttpPost("employees")]
        public async Task<ActionResult<BaseResponseDTO<EmployeeDTO>>> RegisterEmployee([FromBody] EmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponseDTO<EmployeeDTO>.ErrorResult("Invalid request."));

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<EmployeeDTO>.ErrorResult("Invalid client association"));
            }

            model.ClientId = currentClientId;

            var response = await _employeeService.CreateAsync(model);

            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("employees/{id}")]
        public async Task<ActionResult<BaseResponseDTO<EmployeeDTO>>> UpdateEmployee(int id, [FromBody] EmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponseDTO<EmployeeDTO>.ErrorResult("Invalid request."));

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var existingEmployee = await _employeeService.GetByIdAsync(id);
            if (!existingEmployee.Success || existingEmployee.Data?.ClientId != currentClientId)
            {
                return Forbid();
            }

            model.ClientId = currentClientId; // Ensure client ID consistency

            var result = await _employeeService.UpdateAsync(id, model);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("employees/{id}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteEmployee(int id)
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var existingEmployee = await _employeeService.GetByIdAsync(id);
            if (!existingEmployee.Success || existingEmployee.Data?.ClientId != currentClientId)
            {
                return Forbid();
            }

            var result = await _employeeService.DeleteAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("employees/{id}")]
        public async Task<ActionResult<BaseResponseDTO<EmployeeDTO>>> GetEmployeeById(int id)
        {
            var result = await _employeeService.GetByIdAsync(id);

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (!result.Success || result.Data?.ClientId != currentClientId)
            {
                return Forbid();
            }

            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("employees")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<EmployeeDTO>>>> GetAllEmployees()
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<EmployeeDTO>>.ErrorResult("Invalid client association"));
            }

            var allEmployees = await _employeeService.GetAllAsync();

            var clientEmployees = new BaseResponseDTO<IEnumerable<EmployeeDTO>>
            {
                Success = allEmployees.Success,
                Message = allEmployees.Message,
                Data = allEmployees.Data?.Where(e => e.ClientId == currentClientId),
                Errors = allEmployees.Errors
            };

            return Ok(clientEmployees);
        }
    }
}