using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
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

    private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".avi", ".mkv", ".webm", ".mpeg", ".mpg", ".m4v" };

    public DocumentService(IOptions<CloudinarySettings> cloudSettings, BankDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;

        var account = new Account(
            cloudSettings.Value.CloudName,
            cloudSettings.Value.ApiKey,
            cloudSettings.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    private async Task<UploadResult> UploadToCloudinary(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        using var stream = file.OpenReadStream();
        var fileDesc = new FileDescription(file.FileName, stream);

        var contentType = file.ContentType ?? string.Empty;
        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        // 1. IMAGE
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var uploadParams = new ImageUploadParams
            {
                File = fileDesc,
                PublicId = Guid.NewGuid().ToString("N")
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }

        // 2. VIDEO
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) || VideoExtensions.Contains(ext))
        {
            var uploadParams = new VideoUploadParams
            {
                File = fileDesc,
                PublicId = Guid.NewGuid().ToString("N")
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }

        // 3. FALLBACK: RAW (PDF, DOCX, ZIP, etc.)
        var rawParams = new RawUploadParams
        {
            File = fileDesc,
            PublicId = Guid.NewGuid().ToString("N")
        };
        return await _cloudinary.UploadAsync(rawParams);
    }

    public async Task<DocumentDTO> UploadDocumentAsync(IFormFile file, long uploadedBy, long bankId, long? clientId = null, string? docType = null)
    {
        // Upload file to Cloudinary
        var uploadResult = await UploadToCloudinary(file);

        if (uploadResult == null || uploadResult.Error != null)
            throw new Exception(uploadResult?.Error?.Message ?? "Cloudinary upload failed");

        // Save metadata in database
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

        // Map to DTO
        return _mapper.Map<DocumentDTO>(document);
    }
}
