namespace API_BANKING_PAYMENT.Models.DTO
{
    public class DocumentUploadRequest
    {
        public IFormFile File { get; set; }
        public long UploadedBy { get; set; }
        public long BankId { get; set; }
        public long? ClientId { get; set; }
        public string? DocType { get; set; }
    }

    public class DocumentMultipleUploadRequest
    {
        public List<IFormFile> Files { get; set; }
        public long UploadedBy { get; set; }
        public long BankId { get; set; }
        public long? ClientId { get; set; }
        public string? DocType { get; set; }
    }

    public class DocumentUpdateRequest
    {
        public IFormFile NewFile { get; set; } 
        public long DocumentId { get; set; }    
    }

}
