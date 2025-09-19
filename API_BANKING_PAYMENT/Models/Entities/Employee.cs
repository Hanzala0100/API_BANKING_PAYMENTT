using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models.Entities;

public partial class Employee
{
    public long EmployeeId { get; set; }

    public long ClientId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public long AccountNumber { get; set; }

    public string BankName { get; set; } = null!;

    public string Ifsccode { get; set; } = null!;

    public decimal SalaryAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<SalaryDisbursement> SalaryDisbursements { get; set; } = new List<SalaryDisbursement>();
}
