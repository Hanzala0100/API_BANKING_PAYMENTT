using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Settings;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class DocumentService : IDocumentService
{
    private readonly Cloudinary _cloudinary;
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _logger;

    private static readonly string[] VideoExtensions =
        { ".mp4", ".mov", ".avi", ".mkv", ".webm", ".mpeg", ".mpg", ".m4v" };

    public DocumentService(
        IOptions<CloudinarySettings> cloudSettings,
        IDocumentRepository repository,
        IMapper mapper,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _mapper = mapper;

        var account = new Account(
            cloudSettings.Value.CloudName,
            cloudSettings.Value.ApiKey,
            cloudSettings.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
        _logger = logger;
    }

    private async Task<UploadResult> UploadToCloudinary(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload attempt with empty file.");
            throw new ArgumentException("File is empty");
        }

        using var stream = file.OpenReadStream();
        var fileDesc = new FileDescription(file.FileName, stream);

        var contentType = file.ContentType ?? string.Empty;
        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        UploadResult result;

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var uploadParams = new ImageUploadParams
            {
                File = fileDesc,
                PublicId = Guid.NewGuid().ToString("N"),
                AccessMode = "public"
                
            };
            result = await _cloudinary.UploadAsync(uploadParams);
        }
        else if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) || VideoExtensions.Contains(ext))
        {
            var uploadParams = new VideoUploadParams
            {
                File = fileDesc,
                PublicId = Guid.NewGuid().ToString("N"),
                AccessMode = "public"
            };
            result = await _cloudinary.UploadAsync(uploadParams);
        }
        else
        {
            var rawParams = new RawUploadParams
            {
                File = fileDesc,
                PublicId = Guid.NewGuid().ToString("N"),
                AccessMode = "public"
            };
            result = await _cloudinary.UploadAsync(rawParams);
        }

        return result;
    }

    public async Task<BaseResponseDTO<DocumentDTO>> UploadDocumentAsync(IFormFile file, long uploadedBy, long bankId, long? clientId = null, string? docType = null)
    {
        try
        {
            var uploadResult = await UploadToCloudinary(file);

            if (uploadResult == null || uploadResult.Error != null)
            {
                var errorMsg = uploadResult?.Error?.Message ?? "Upload failed";
                _logger.LogError("Cloudinary upload failed: {Error}", errorMsg);
                return BaseResponseDTO<DocumentDTO>.ErrorResult(errorMsg);
            }

            var document = new Document
            {
                UploadedBy = uploadedBy,
                BankId = bankId,
                ClientId = clientId,
                DocType = docType,
                FileName = file.FileName,
                FileUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString(),
                UploadedAt = DateTime.UtcNow
            };

            await _repository.Add(document);

            _logger.LogInformation("Document saved in DB. DocumentId: {DocumentId}, FileName: {FileName}",
                document.DocumentId, document.FileName);

            var dto = _mapper.Map<DocumentDTO>(document);
            dto.DocumentId = document.DocumentId;
            return BaseResponseDTO<DocumentDTO>.SuccessResult(dto, "Document uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document upload.");
            return BaseResponseDTO<DocumentDTO>.ErrorResult("An error occurred while uploading document.", new() { ex.Message });
        }
    }

    public async Task<BaseResponseDTO<bool>> DeleteDocumentAsync(long documentId)
    {
        try
        {
            var document = await _repository.GetById(documentId);

            if (document == null)
            {
                return new BaseResponseDTO<bool>
                {
                    Success = false,
                    Message = "Document not found.",
                    Data = false
                };
            }

            _repository.Delete(document);
            _logger.LogInformation("Document with ID {DocumentId} deleted successfully.", documentId);
            return new BaseResponseDTO<bool>
            {
                Success = true,
                Message = "Document deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting document with ID {DocumentId}", documentId);

            return new BaseResponseDTO<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the document.",
                Data = false
            };
        }
    }

    public async Task<BaseResponseDTO<DocumentDTO>> GetDocumentByIdAsync(long documentId)
    {
        try
        {
            var document = await _repository.GetById(documentId);

            if (document == null)
            {
                return new BaseResponseDTO<DocumentDTO>
                {
                    Success = false,
                    Message = "Document not found.",
                    Data = null
                };
            }

            var documentDto = new DocumentDTO
            {
                DocumentId = document.DocumentId,
                FileName = document.FileName,
                UploadedBy = document.UploadedBy,
                BankId = document.BankId,
                ClientId = document.ClientId,
                DocType = document.DocType,
                UploadedAt = document.UploadedAt,
                FileUrl = document.FileUrl
            };
            _logger.LogInformation("Document with ID {DocumentId} retrieved successfully.", documentId);
            return new BaseResponseDTO<DocumentDTO>
            {
                Success = true,
                Message = "Document retrieved successfully.",
                Data = documentDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving document with ID {DocumentId}", documentId);

            return new BaseResponseDTO<DocumentDTO>
            {
                Success = false,
                Message = "An error occurred while retrieving the document.",
                Data = null
            };
        }
    }

    public async Task<BaseResponseDTO<DocumentDTO>> UpdateDocumentAsync(long documentId, IFormFile newFile)
    {
        try
        {
            var document = await _repository.GetById(documentId);

            if (document == null)
            {
                return new BaseResponseDTO<DocumentDTO>
                {
                    Success = false,
                    Message = "Document not found.",
                    Data = null
                };
            }

            var uploadResult = await UploadToCloudinary(newFile);

            if (uploadResult == null || uploadResult.Error != null)
            {
                var errorMsg = uploadResult?.Error?.Message ?? "Upload failed";
                _logger.LogError("Cloudinary upload failed during update: {Error}", errorMsg);

                return BaseResponseDTO<DocumentDTO>.ErrorResult(errorMsg);
            }

            document.FileName = newFile.FileName;
            document.FileUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
            document.UploadedAt = DateTime.UtcNow;

            _repository.Update(document);

            var dto = _mapper.Map<DocumentDTO>(document);

            _logger.LogInformation(" Document updated successfully. DocumentId: {DocumentId}, FileName: {FileName}",
                document.DocumentId, document.FileName);

            return BaseResponseDTO<DocumentDTO>.SuccessResult(dto, "Document updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating document with ID {DocumentId}", documentId);

            return BaseResponseDTO<DocumentDTO>.ErrorResult(
                "An error occurred while updating the document.",
                new() { ex.Message }
            );
        }
    }

}
