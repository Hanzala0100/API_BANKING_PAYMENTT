namespace API_BANKING_PAYMENT.Models.DTO
{
    public class BulkEmployeeImportDTO
    {
        public IFormFile CsvFile { get; set; }
    }

    public class BulkEmployeeImportResponseDTO
    {
        public int TotalRecords { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<EmployeeDTO> ImportedEmployees { get; set; } = new List<EmployeeDTO>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class CsvEmployeeRecordDTO
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public long AccountNumber { get; set; }
        public string BankName { get; set; }
        public string Ifsccode { get; set; }
        public decimal SalaryAmount { get; set; }
    }
}