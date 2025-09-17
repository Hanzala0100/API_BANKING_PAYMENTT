using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models;

public partial class Beneficiary
{
    public long BeneficiaryId { get; set; }

    public long ClientId { get; set; }

    public string FullName { get; set; } = null!;

    public long AccountNumber { get; set; }

    public string BankName { get; set; } = null!;

    public string Ifsccode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
