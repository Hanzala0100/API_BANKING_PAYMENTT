using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Enum;
using API_BANKING_PAYMENT.Services;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "BankUser")]
    public class BankUserController : ControllerBase
    {
        private readonly IBankUserService _bankUserService;
        private readonly ILogger<BankUserController> _logger;
        private readonly IPaymentService _paymentService;


        public BankUserController(
            IBankUserService bankUserService, 
            ILogger<BankUserController> logger,
            IPaymentService paymentService
            )
        {
            _bankUserService = bankUserService;
            _logger = logger;
            _paymentService = paymentService;
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

        // CLIENT VERIFICATION ENDPOINTS

        [HttpPut("clients/{clientId}/verify")]
        public async Task<ActionResult<BaseResponseDTO<ClientDTO>>> VerifyClient(long clientId, [FromBody] VerifyClientRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<ClientDTO>.ErrorResult("Invalid input data"));
            }

            if (clientId <= 0)
            {
                return BadRequest(BaseResponseDTO<ClientDTO>.ErrorResult("Invalid client ID"));
            }

            var currentUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserBankId = long.Parse(User.FindFirst("BankId")?.Value ?? "0");

            var result = await _bankUserService.VerifyClientAsync(clientId, currentUserId, currentUserBankId, request.VerificationStatus, request.Notes);

            if (result.Success)
            {
                _logger.LogInformation("Client verification updated: {ClientId}, Status: {Status}", clientId, request.VerificationStatus);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Client verification failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("clients/verification-status/{status}")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<ClientDTO>>>> GetClientsByVerificationStatus(string status)
        {
            if (string.IsNullOrEmpty(status) || !VerificationStatus.GetAllStatuses().Contains(status))
            {
                return BadRequest(BaseResponseDTO<IEnumerable<ClientDTO>>.ErrorResult("Invalid verification status"));
            }

            var result = await _bankUserService.GetClientsByVerificationStatusAsync(status);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("clients/pending-verification")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<ClientDTO>>>> GetClientsWithPendingVerification()
        {
            var result = await _bankUserService.GetClientsWithPendingVerificationAsync();

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        // CLIENT DOCUMENT ENDPOINTS

        [HttpPost("clients/{clientId}/documents")]
        public async Task<ActionResult<BaseResponseDTO<DocumentDTO>>> UploadClientDocument(long clientId, [FromForm] UploadDocumentRequestDTO request)
        {
            if (clientId <= 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid client ID"));
            }

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("File is required"));
            }

            if (string.IsNullOrEmpty(request.DocType))
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Document type is required"));
            }

            // Get current user ID and bank ID from claims
            var currentUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserBankId = long.Parse(User.FindFirst("BankId")?.Value ?? "0");

            var result = await _bankUserService.UploadClientDocumentAsync(clientId, request.File, currentUserId, currentUserBankId, request.DocType);

            if (result.Success)
            {
                _logger.LogInformation("Document uploaded successfully for client ID: {ClientId}", clientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Document upload failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("clients/{clientId}/documents")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<DocumentDTO>>>> GetClientDocuments(long clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Invalid client ID"));
            }

            var result = await _bankUserService.GetClientDocumentsAsync(clientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        // CLIENT USER ENDPOINTS

        [HttpPost("clients/{clientId}/users")]
        public async Task<ActionResult<BaseResponseDTO<ClientUserCreationDTO>>> CreateClientUser(long clientId, [FromBody] RegisterDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<ClientUserCreationDTO>.ErrorResult("Invalid input data"));
            }

            userDTO.ClientId = clientId;
            var result = await _bankUserService.CreateClientUserAsync(userDTO);

            if (result.Success)
            {
                _logger.LogInformation("Client user created successfully for client ID: {ClientId}", clientId);
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



        // PAYMENT APPROVAL 
        [HttpGet("payments/pending")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<PaymentDTO>>>> GetPendingPayments()
        {
            var result = await _paymentService.GetPendingPaymentsAsync();

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("payments")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<PaymentDTO>>>> GetPaymentsByStatus([FromQuery] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest(BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Status parameter is required"));
            }

            var result = await _paymentService.GetPaymentsByStatusAsync(status);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPut("payments/{paymentId}/approve")]
        public async Task<ActionResult<BaseResponseDTO<PaymentDTO>>> ApprovePayment(long paymentId, [FromBody] ApprovePaymentRequestDTO request)
        {
            if (paymentId <= 0)
            {
                return BadRequest(BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid payment ID"));
            }

            var currentBankUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _paymentService.ApprovePaymentAsync(paymentId, currentBankUserId, request.Notes);

            if (result.Success)
            {
                _logger.LogInformation("Payment approved successfully: {PaymentId}, ApprovedBy: {BankUserId}", paymentId, currentBankUserId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Payment approval failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpPut("payments/{paymentId}/reject")]
        public async Task<ActionResult<BaseResponseDTO<PaymentDTO>>> RejectPayment(long paymentId, [FromBody] ApprovePaymentRequestDTO request)
        {
            if (paymentId <= 0)
            {
                return BadRequest(BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid payment ID"));
            }

            var currentBankUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _paymentService.RejectPaymentAsync(paymentId, currentBankUserId, request.Notes);

            if (result.Success)
            {
                _logger.LogInformation("Payment rejected: {PaymentId}, RejectedBy: {BankUserId}", paymentId, currentBankUserId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Payment rejection failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("payments/{paymentId}")]
        public async Task<ActionResult<BaseResponseDTO<PaymentDTO>>> GetPaymentById(long paymentId)
        {
            if (paymentId <= 0)
            {
                return BadRequest(BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid payment ID"));
            }

            var result = await _paymentService.GetPaymentByIdAsync(paymentId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

         
       
    }
}