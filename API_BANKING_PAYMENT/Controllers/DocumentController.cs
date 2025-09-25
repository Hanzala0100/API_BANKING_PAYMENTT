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
        public async Task<IActionResult> UploadDocument(
            IFormFile file,
            [FromQuery] long uploadedBy,
            [FromQuery] long bankId,
            [FromQuery] long? clientId = null,
            [FromQuery] string? docType = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest(BaseResponseDTO<DocumentDTO>.ErrorResult("File is required."));

            var result = await _documentService.UploadDocumentAsync(file, uploadedBy, bankId, clientId, docType);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleDocuments(
            List<IFormFile> files,
            [FromQuery] long uploadedBy,
            [FromQuery] long bankId,
            [FromQuery] long? clientId = null,
            [FromQuery] string? docType = null)
        {
            if (files == null || files.Count == 0)
                return BadRequest(BaseResponseDTO<List<DocumentDTO>>.ErrorResult("At least one file is required."));

            var results = new List<DocumentDTO>();

            foreach (var file in files)
            {
                var response = await _documentService.UploadDocumentAsync(file, uploadedBy, bankId, clientId, docType);

                if (response.Success && response.Data != null)
                    results.Add(response.Data);
                else
                    return BadRequest(response); 
            }

            return Ok(BaseResponseDTO<List<DocumentDTO>>.SuccessResult(results, "All documents uploaded successfully."));
        }
    }
}
