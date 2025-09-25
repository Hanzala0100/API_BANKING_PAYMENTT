using API_BANKING_PAYMENT.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_BANKING_PAYMENT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("File is required."));

            var result = await _documentService.UploadDocumentAsync(
                request.File, request.UploadedBy, request.BankId, request.ClientId, request.DocType
            );

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("upload-multiple")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMultipleDocuments([FromForm] DocumentMultipleUploadRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
                return BadRequest(BaseResponseDTO<List<DocumentDTO>>.ErrorResult("At least one file is required."));

            var results = new List<DocumentDTO>();

            foreach (var file in request.Files)
            {
                var response = await _documentService.UploadDocumentAsync(
                    file, request.UploadedBy, request.BankId, request.ClientId, request.DocType
                );

                if (response.Success && response.Data != null)
                    results.Add(response.Data);
                else
                    return BadRequest(response);
            }

            return Ok(BaseResponseDTO<List<DocumentDTO>>.SuccessResult(results, "All documents uploaded successfully."));
        }


        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetDocumentById(long documentId)
        {
            var result = await _documentService.GetDocumentByIdAsync(documentId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(long documentId)
        {
            var result = await _documentService.DeleteDocumentAsync(documentId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateDocument([FromForm] DocumentUpdateRequest request)
        {
            if (request.NewFile == null || request.NewFile.Length == 0)
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("File is required for update."));

            var result = await _documentService.UpdateDocumentAsync(request.DocumentId, request.NewFile);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}
