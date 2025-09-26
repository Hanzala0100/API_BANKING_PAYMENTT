namespace API_BANKING_PAYMENT.Models.DTO
{
    public class SalaryDisbursementDTO
    {
        public long SalaryId { get; set; }
        public long ClientId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime DisbursementDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; }
    }

    public class CreateSalaryDisbursementDTO
    {
        public long ClientId { get; set; }
        public long EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DisbursementDate { get; set; } = DateTime.UtcNow;
    }

    public class BatchSalaryDisbursementDTO
    {
        public long ClientId { get; set; }
        public List<BatchEmployeeSalaryDTO> Employees { get; set; } = new List<BatchEmployeeSalaryDTO>();
        public DateTime DisbursementDate { get; set; } = DateTime.UtcNow;
    }

    public class BatchEmployeeSalaryDTO
    {
        public long EmployeeId { get; set; }
        public decimal Amount { get; set; } // Custom amount for this disbursement
    }

    public class SalaryDisbursementResponseDTO
    {
        public long SalaryId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string TransactionReference { get; set; }
    }
}