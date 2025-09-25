namespace API_BANKING_PAYMENT.Models.DTO
{
    
        public class VerifyClientRequestDTO
        {
            public string VerificationStatus { get; set; }
            public string Notes { get; set; }
        }

        public class UploadDocumentRequestDTO
        {
            public IFormFile File { get; set; }
            public string DocType { get; set; }
        }
}
