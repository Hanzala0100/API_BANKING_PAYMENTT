using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models;

public partial class SalaryDisbursement
{
    public long SalaryId { get; set; }

    public long ClientId { get; set; }

    public long EmployeeId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime DisbursementDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
