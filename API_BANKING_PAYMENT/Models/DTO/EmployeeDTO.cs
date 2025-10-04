namespace API_BANKING_PAYMENT.Models.DTO
{
    //emp dto
    public class EmployeeDTO
    {
        public long EmployeeId { get; set; }
        public long ClientId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public long AccountNumber { get; set; }
        public string BankName { get; set; } = null!;
        public string Ifsccode { get; set; } = null!;
        public decimal SalaryAmount { get; set; }
    }
}
