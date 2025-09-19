using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models.Entities;

public partial class Payment
{
    public long PaymentId { get; set; }

    public long? ClientId { get; set; }

    public long? BeneficiaryId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Beneficiary? Beneficiary { get; set; }

    public virtual Client? Client { get; set; }
}
