namespace API_BANKING_PAYMENT.Models.DTO
{
    public class EmailRequestDTO
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<string>? CcEmails { get; set; }
        public List<string>? BccEmails { get; set; }
        public Dictionary<string, string>? TemplateParameters { get; set; }
    }

    public class ClientEmailRequestDTO
    {
        public long ClientId { get; set; }
        public string EmailType { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}