namespace API_BANKING_PAYMENT.Models.DTO
{
    public class BatchSalaryDisbursementResponseDTO
    {
        public int TotalProcessed { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<SalaryDisbursementDTO> ProcessedSalaries { get; set; } = new List<SalaryDisbursementDTO>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
