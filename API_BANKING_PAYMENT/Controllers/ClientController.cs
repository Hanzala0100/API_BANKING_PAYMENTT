using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services;
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
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly ISalaryDisbursementService _salaryDisbursementService;
        private readonly IPaymentService _paymentService;
        private readonly IClientService _clientService;

        public ClientController(
            IEmployeeService employeeService, 
            IBankUserService bankUserService, 
            ILogger<ClientController> logger, 
            IBeneficiaryService beneficiaryService,
            ISalaryDisbursementService salaryDisbursementService,
            IPaymentService paymentService,
            IClientService clientService
            )
        {
            _employeeService = employeeService;
            _bankUserService = bankUserService;
            _logger = logger;
            _beneficiaryService = beneficiaryService;
            _salaryDisbursementService = salaryDisbursementService;
            _paymentService = paymentService;
            _clientService = clientService;
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

        // BENEFICIARY MANAGEMENT ENDPOINTS

        [HttpPost("beneficiaries")]
        public async Task<ActionResult<BaseResponseDTO<BeneficiaryDTO>>> CreateBeneficiary([FromBody] BeneficiaryRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Invalid input data"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Invalid client association"));
            }

            model.ClientId = currentClientId;

            var result = await _beneficiaryService.CreateAsync(model);

            if (result.Success)
            {
                _logger.LogInformation("Beneficiary created successfully for client ID: {ClientId}", currentClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Beneficiary creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("beneficiaries")]
        public async Task<ActionResult<BaseResponseDTO<List<BeneficiaryDTO>>>> GetBeneficiaries()
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<List<BeneficiaryDTO>>.ErrorResult("Invalid client association"));
            }

            var result = await _beneficiaryService.GetByClientIdAsync(currentClientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("beneficiaries/{id}")]
        public async Task<ActionResult<BaseResponseDTO<BeneficiaryDTO>>> GetBeneficiaryById(long id)
        {
            if (id <= 0)
            {
                return BadRequest(BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Invalid beneficiary ID"));
            }

            var result = await _beneficiaryService.GetByIdAsync(id);

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (result.Success && result.Data?.ClientId != currentClientId)
            {
                return Forbid();  
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

        [HttpPut("beneficiaries/{id}")]
        public async Task<ActionResult<BaseResponseDTO<BeneficiaryDTO>>> UpdateBeneficiary(long id, [FromBody] BeneficiaryDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Invalid input data"));
            }

            if (id != model.BeneficiaryId)
            {
                return BadRequest(BaseResponseDTO<BeneficiaryDTO>.ErrorResult("Beneficiary ID mismatch"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var existingBeneficiary = await _beneficiaryService.GetByIdAsync(id);
            if (!existingBeneficiary.Success || existingBeneficiary.Data?.ClientId != currentClientId)
            {
                return Forbid();
            }

            model.ClientId = currentClientId;

            var result = await _beneficiaryService.UpdateAsync(id, model);

            if (result.Success)
            {
                _logger.LogInformation("Beneficiary updated successfully: {BeneficiaryId}", id);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Beneficiary update failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpDelete("beneficiaries/{id}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteBeneficiary(long id)
        {
            if (id <= 0)
            {
                return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid beneficiary ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var existingBeneficiary = await _beneficiaryService.GetByIdAsync(id);
            if (!existingBeneficiary.Success || existingBeneficiary.Data?.ClientId != currentClientId)
            {
                return Forbid();
            }

            var result = await _beneficiaryService.DeleteAsync(id);

            if (result.Success)
            {
                _logger.LogInformation("Beneficiary deleted successfully: {BeneficiaryId}", id);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Beneficiary deletion failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }



        // PAYMENT MANAGEMENT ENDPOINTS  
        [HttpPost("payments")]
        public async Task<ActionResult<BaseResponseDTO<PaymentDTO>>> CreatePayment([FromBody] CreatePaymentDTO paymentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid input data"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid client association"));
            }

            paymentDTO.ClientId = currentClientId;

            var result = await _paymentService.CreatePaymentAsync(paymentDTO);

            if (result.Success)
            {
                _logger.LogInformation("Payment created successfully for client ID: {ClientId}", currentClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Payment creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("payments")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<PaymentDTO>>>> GetPayments()
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<PaymentDTO>>.ErrorResult("Invalid client association"));
            }

            var result = await _paymentService.GetPaymentsByClientIdAsync(currentClientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("payments/{paymentId}")]
        public async Task<ActionResult<BaseResponseDTO<PaymentDTO>>> GetPaymentById(long paymentId)
        {
            if (paymentId <= 0)
            {
                return BadRequest(BaseResponseDTO<PaymentDTO>.ErrorResult("Invalid payment ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var paymentResult = await _paymentService.GetPaymentByIdAsync(paymentId);

            if (paymentResult.Success && paymentResult.Data?.ClientId != currentClientId)
            {
                return Forbid();  
            }

            if (paymentResult.Success)
            {
                return Ok(paymentResult);
            }
            else
            {
                return NotFound(paymentResult);
            }
        }

        [HttpDelete("payments/{paymentId}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeletePayment(long paymentId)
        {
            if (paymentId <= 0)
            {
                return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid payment ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var paymentResult = await _paymentService.GetPaymentByIdAsync(paymentId);

            if (!paymentResult.Success || paymentResult.Data?.ClientId != currentClientId)
            {
                return Forbid(); 
            }

            var result = await _paymentService.DeletePaymentAsync(paymentId);

            if (result.Success)
            {
                _logger.LogInformation("Payment deleted successfully: {PaymentId}", paymentId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Payment deletion failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        // SALARY DISBURSEMENT  
        [HttpPost("salary-disbursements")]
        public async Task<ActionResult<BaseResponseDTO<SalaryDisbursementDTO>>> CreateSalaryDisbursement([FromBody] CreateSalaryDisbursementDTO disbursementDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Invalid input data"));
            }

            // Get current user's client ID from claims and set it
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Invalid client association"));
            }

            disbursementDTO.ClientId = currentClientId;

            var result = await _salaryDisbursementService.CreateSalaryDisbursementAsync(disbursementDTO);

            if (result.Success)
            {
                _logger.LogInformation("Salary disbursement created successfully for client ID: {ClientId}", currentClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Salary disbursement creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpPost("salary-disbursements/batch")]
        public async Task<ActionResult<BaseResponseDTO<BatchSalaryDisbursementResponseDTO>>> CreateBatchSalaryDisbursement([FromBody] BatchSalaryDisbursementDTO batchDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponseDTO<BatchSalaryDisbursementResponseDTO>.ErrorResult("Invalid input data"));
            }

            // Get current user's client ID from claims and set it
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<BatchSalaryDisbursementResponseDTO>.ErrorResult("Invalid client association"));
            }

            batchDTO.ClientId = currentClientId;

            var result = await _salaryDisbursementService.CreateBatchSalaryDisbursementAsync(batchDTO);

            if (result.Success)
            {
                _logger.LogInformation("Batch salary disbursement created successfully for client ID: {ClientId}", currentClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Batch salary disbursement creation failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("salary-disbursements")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>>> GetSalaryDisbursements()
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<SalaryDisbursementDTO>>.ErrorResult("Invalid client association"));
            }

            var result = await _salaryDisbursementService.GetSalaryDisbursementsByClientIdAsync(currentClientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPost("salary-disbursements/{salaryId}/process")]
        public async Task<ActionResult<BaseResponseDTO<SalaryDisbursementDTO>>> ProcessSalaryDisbursement(long salaryId)
        {
            if (salaryId <= 0)
            {
                return BadRequest(BaseResponseDTO<SalaryDisbursementDTO>.ErrorResult("Invalid salary ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var salaryResult = await _salaryDisbursementService.GetSalaryDisbursementByIdAsync(salaryId);

            if (!salaryResult.Success || salaryResult.Data?.ClientId != currentClientId)
            {
                return Forbid(); 
            }

            var result = await _salaryDisbursementService.ProcessSalaryDisbursementAsync(salaryId);

            if (result.Success)
            {
                _logger.LogInformation("Salary disbursement processed successfully: {SalaryId}", salaryId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Salary disbursement processing failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }


 

        // DOCUMENT MANAGEMENT  
        [HttpPost("documents")]
        public async Task<ActionResult<BaseResponseDTO<DocumentDTO>>> UploadDocument([FromForm] UploadDocumentRequestDTO request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("File is required"));
            }

            if (string.IsNullOrEmpty(request.DocType))
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Document type is required"));
            }

            // Get current user's client ID and user ID from claims
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            var currentUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid client association"));
            }

            var result = await _clientService.UploadClientDocumentAsync(currentClientId, request.File, currentUserId, request.DocType);

            if (result.Success)
            {
                _logger.LogInformation("Document uploaded successfully for client ID: {ClientId}", currentClientId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Document upload failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpGet("documents")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<DocumentDTO>>>> GetDocuments()
        {
            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Invalid client association"));
            }

            var result = await _clientService.GetClientDocumentsAsync(currentClientId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("documents/{documentId}")]
        public async Task<ActionResult<BaseResponseDTO<DocumentDTO>>> GetDocumentById(long documentId)
        {
            if (documentId <= 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid document ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid client association"));
            }

            var result = await _clientService.GetClientDocumentByIdAsync(currentClientId, documentId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpGet("documents/type/{docType}")]
        public async Task<ActionResult<BaseResponseDTO<IEnumerable<DocumentDTO>>>> GetDocumentsByType(string docType)
        {
            if (string.IsNullOrEmpty(docType))
            {
                return BadRequest(BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Document type is required"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Invalid client association"));
            }

            var result = await _clientService.GetClientDocumentsByTypeAsync(currentClientId, docType);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPut("documents/{documentId}")]
        public async Task<ActionResult<BaseResponseDTO<DocumentDTO>>> UpdateDocument(long documentId, [FromForm] UpdateClientDocumentRequestDTO request)
        {
            if (documentId <= 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid document ID"));
            }

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("File is required"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("Invalid client association"));
            }

            var result = await _clientService.UpdateClientDocumentAsync(currentClientId, documentId, request.File);

            if (result.Success)
            {
                _logger.LogInformation("Document updated successfully: {DocumentId}", documentId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Document update failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        [HttpDelete("documents/{documentId}")]
        public async Task<ActionResult<BaseResponseDTO<bool>>> DeleteDocument(long documentId)
        {
            if (documentId <= 0)
            {
                return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid document ID"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<bool>.ErrorResult("Invalid client association"));
            }

            var result = await _clientService.DeleteClientDocumentAsync(currentClientId, documentId);

            if (result.Success)
            {
                _logger.LogInformation("Document deleted successfully: {DocumentId}", documentId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Document deletion failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }

        //Bulk Import via csv

        [HttpPost("employees/bulk-import")]
        public async Task<ActionResult<BaseResponseDTO<BulkEmployeeImportResponseDTO>>> BulkImportEmployees([FromForm] BulkEmployeeImportDTO request)
        {
            if (request.CsvFile == null || request.CsvFile.Length == 0)
            {
                return BadRequest(BaseResponseDTO<BulkEmployeeImportResponseDTO>.ErrorResult("CSV file is required"));
            }

            var currentClientId = long.Parse(User.FindFirst("ClientId")?.Value ?? "0");
            if (currentClientId <= 0)
            {
                return BadRequest(BaseResponseDTO<BulkEmployeeImportResponseDTO>.ErrorResult("Invalid client association"));
            }

            var response = await _employeeService.BulkImportEmployeesAsync(currentClientId, request.CsvFile);

            if (response.Success)
            {
                _logger.LogInformation("Bulk employee import successful for client ID: {ClientId}. Imported: {Imported}, Failed: {Failed}",
                    currentClientId, response.Data.Successful, response.Data.Failed);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Bulk employee import failed for client ID: {ClientId}. Error: {Message}",
                    currentClientId, response.Message);
                return BadRequest(response);
            }
        }

        //Employee Paginated testing
        [HttpGet("paginated-employee")]
        public async Task<IActionResult> GetPaginatedEmployee([FromQuery] PaginationRequestDTO paginationRequest)
        {
            var result = await _employeeService.GetAllPaginatedAsync(paginationRequest);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        //Beneficiary Paginated

        [HttpGet("paginated-beneficiary")]
        public async Task<IActionResult> GetPaginatedBeneficiary([FromQuery] PaginationRequestDTO paginationRequest)
        {
            var result = await _beneficiaryService.GetAllPaginatedAsync(paginationRequest);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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

    }


}