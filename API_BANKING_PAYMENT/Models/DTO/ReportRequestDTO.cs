namespace API_BANKING_PAYMENT.Models.DTO
{
    public class ReportRequestDTO
    {
        public long GeneratedBy { get; set; }     
        public string ReportType { get; set; } = string.Empty; 
    }
}
