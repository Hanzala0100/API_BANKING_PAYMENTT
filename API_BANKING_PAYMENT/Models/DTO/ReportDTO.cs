using API_BANKING_PAYMENT.Models.Enum;

namespace API_BANKING_PAYMENT.Models.DTO
{
    public class ReportDTO
    {
        public long ReportId { get; set; }
        public string ReportType { get; set; } 
        public DateTime GeneratedAt { get; set; }
        public string? FileUrl { get; set; }
        public long GeneratedBy { get; set; }
        public string? GeneratedByName { get; set; }
    }
}