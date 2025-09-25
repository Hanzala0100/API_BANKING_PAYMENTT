using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;

namespace API_BANKING_PAYMENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "BankUser")]
    public class BankUserController : ControllerBase
    {
        private readonly IBankUserService _bankUserService;
        private readonly ILogger<BankUserController> _logger;

        public BankUserController(IBankUserService bankUserService, ILogger<BankUserController> logger)
        {
            _bankUserService = bankUserService;
            _logger = logger;
        }

        // CLIENT ENDPOINTS

        [HttpPost("clients")]
        public async Task<ActionResult<BaseResponseDTO<ClientDTO>>> CreateClient([FromBody] ClientCreationDTO clientDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<ClientDTO>.ErrorResult("Invalid input data"));
            }

            var result = await _bankUserService.CreateClientAsync(clientDTO);

            if (result.Success)
            {
                _logger.LogInformation("Client created successfully: {ClientName}", clientDTO.ClientName);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("clients")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<ClientDTO>>>> GetAllClients()
        {
            var result = await _bankUserService.GetAllClientsAsync();

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("clients/{clientId}")]
        public async Task<ActionResult<BaseResponseDTO<ClientDTO>>> GetClientById(long clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest(BaseResponseDTO<ClientDTO>.ErrorResult("Invalid client ID"));
            }

            var result = await _bankUserService.GetClientByIdAsync(clientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpPut("clients/{clientId}")]
        public async Task<ActionResult<BaseResponseDTO<ClientDTO>>> UpdateClient(long clientId, [FromBody] ClientDTO clientDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<ClientDTO>.ErrorResult("Invalid input data"));
            }

            if (clientId != clientDTO.ClientId)
            {
                return BadRequest(BaseResponseDTO<ClientDTO>.ErrorResult("Client ID mismatch"));
            }

            var result = await _bankUserService.UpdateClientAsync(clientDTO);

            if (result.Success)
            {
                _logger.LogInformation("Client updated successfully: {ClientId}", clientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client update failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpDelete("clients/{clientId}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteClient(long clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid client ID"));
            }

            var result = await _bankUserService.DeleteClientAsync(clientId);

            if (result.Success)
            {
                _logger.LogInformation("Client deleted successfully: {ClientId}", clientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client deletion failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        // CLIENT USER ENDPOINTS

        [HttpPost("clients/{clientId}/users")]
        public async Task<ActionResult<BaseResponseDTO<UserDTO>>> CreateClientUser(long clientId, [FromBody] RegisterDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<UserDTO>.ErrorResult("Invalid input data"));
            }

            userDTO.ClientId = clientId;
            var result = await _bankUserService.CreateClientUserAsync(userDTO);

            if (result.Success)
            {
                _logger.LogInformation("Client user created successfully for client ID: {ClientId}", result.Data.ClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client user creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("clients/{clientId}/users")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<UserDTO>>>> GetClientUsersByClientId(long clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<UserDTO>>.ErrorResult("Invalid client ID"));
            }

            var result = await _bankUserService.GetAllClientUsersByClientIdAsync(clientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpGet("clients/{clientId}/users/{userId}")]
        public async Task<ActionResult<BaseResponseDTO<UserDTO>>> GetClientUserById(long clientId, long userId)
        {
            if (userId <= 0)
            {
                return BadRequest(BaseResponseDTO<UserDTO>.ErrorResult("Invalid user ID"));
            }

            var result = await _bankUserService.GetClienUserByIdAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpDelete("clients/{clientId}/users/{userId}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteClientUser(long clientId, long userId)
        {
            if (userId <= 0)
            {
                return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid user ID"));
            }

            var result = await _bankUserService.DeleteClientUserAsync(userId);

            if (result.Success)
            {
                _logger.LogInformation("Client user deleted successfully: {UserId}", userId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client user deletion failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }
    }
}
