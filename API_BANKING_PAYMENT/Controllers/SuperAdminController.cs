using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can access
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _superAdminService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(ISuperAdminService superAdminService, ILogger<SuperAdminController> logger)
        {
            _superAdminService = superAdminService;
            _logger = logger;
        }

        // POST: api/superadmin/banks
        [HttpPost("banks")]
        public async Task<ActionResult<BaseResponseDTO<BankDTO>>> CreateBank([FromBody] BankCreationDTO bankCreationDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(BaseResponseDTO<BankDTO>.ErrorResult("Invalid input data"));
                }

                var result = await _superAdminService.CreateBankAsync(bankCreationDTO);

                if (result.Success)
                {
                    _logger.LogInformation("Bank created successfully: {BankName}", bankCreationDTO.BankName);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Bank creation failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bank");
                return StatusCode(500, BaseResponseDTO<BankDTO>.ErrorResult("Internal server error"));
            }
        }

        // GET: api/superadmin/banks
        [HttpGet("banks")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<BankDTO>>>> GetAllBanks()
        {
            try
            {
                var result = await _superAdminService.GetAllBanksAsync();

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all banks");
                return StatusCode(500, BaseResponseDTO<IEnumerable<BankDTO>>.ErrorResult("Internal server error"));
            }
        }

        // GET: api/superadmin/banks/{id}
        [HttpGet("banks/{id}")]
        public async Task<ActionResult<BaseResponseDTO<BankDTO>>> GetBankById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(BaseResponseDTO<BankDTO>.ErrorResult("Invalid bank ID"));
                }

                var result = await _superAdminService.GetBankByIdAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bank with ID: {BankId}", id);
                return StatusCode(500, BaseResponseDTO<BankDTO>.ErrorResult("Internal server error"));
            }
        }

        // DELETE: api/superadmin/banks/{id}
        [HttpDelete("banks/{id}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteBank(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid bank ID"));
                }

                var result = await _superAdminService.DeleteBankAsync(id);

                if (result.Success)
                {
                    _logger.LogInformation("Bank deleted successfully: {BankId}", id);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Bank deletion failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bank with ID: {BankId}", id);
                return StatusCode(500, BaseResponseDTO<bool>.ErrorResult("Internal server error"));
            }
        }
    }
}