namespace API_BANKING_PAYMENT.Models.DTO
{
    public class DocumentDTO
    {
        public long? DocumentId { get; set; }  
        public long UploadedBy { get; set; }
        public long BankId { get; set; }
        public long? ClientId { get; set; }
        public string? DocType { get; set; }

        public string FileName { get; set; } = null!;  
        public string FileUrl { get; set; } = null!;   
        public DateTime UploadedAt { get; set; }      
    }


}
