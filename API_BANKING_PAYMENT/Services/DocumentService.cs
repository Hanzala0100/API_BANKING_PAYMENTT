using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Settings;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class DocumentService : IDocumentService
{
    private readonly Cloudinary _cloudinary;
    private readonly BankDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _logger;

    private static readonly string[] VideoExtensions =
        { ".mp4", ".mov", ".avi", ".mkv", ".webm", ".mpeg", ".mpg", ".m4v" };

    public DocumentService(
        IOptions<CloudinarySettings> cloudSettings,
        BankDbContext dbContext,
        IMapper mapper,
        ILogger<DocumentService> logger)
    {
        _dbContext = dbContext;
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
        try
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
                    PublicId = Guid.NewGuid().ToString("N")
                };
                result = await _cloudinary.UploadAsync(uploadParams);
            }
            else if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) || VideoExtensions.Contains(ext))
            {
                var uploadParams = new VideoUploadParams
                {
                    File = fileDesc,
                    PublicId = Guid.NewGuid().ToString("N")
                };
                result = await _cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                var rawParams = new RawUploadParams
                {
                    File = fileDesc,
                    PublicId = Guid.NewGuid().ToString("N")
                };
                result = await _cloudinary.UploadAsync(rawParams);
            }

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary upload failed for {FileName}: {Error}",
                                 file.FileName, result.Error.Message);
            }
            else
            {
                _logger.LogInformation("Successfully uploaded file {FileName}, PublicId: {PublicId}",
                                       file.FileName, result.PublicId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during upload for file {FileName}", file?.FileName);
            throw;
        }
    }

    public async Task<DocumentDTO> UploadDocumentAsync(IFormFile file, long uploadedBy, long bankId, long? clientId = null, string? docType = null)
    {
        try
        {
            _logger.LogInformation("Starting document upload process. UploadedBy: {UploadedBy}, BankId: {BankId}, ClientId: {ClientId}, DocType: {DocType}",
                                   uploadedBy, bankId, clientId, docType);

            var uploadResult = await UploadToCloudinary(file);

            if (uploadResult == null || uploadResult.Error != null)
            {
                throw new Exception(uploadResult?.Error?.Message ?? "Upload failed");
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

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Document saved in database. DocumentId: {DocumentId}, FileName: {FileName}",
                                   document.DocumentId, document.FileName);

            return _mapper.Map<DocumentDTO>(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document. UploadedBy: {UploadedBy}, BankId: {BankId}", uploadedBy, bankId);
            throw;
        }
    }
}
