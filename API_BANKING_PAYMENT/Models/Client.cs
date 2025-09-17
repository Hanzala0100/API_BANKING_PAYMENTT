using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models;

public partial class Client
{
    public long ClientId { get; set; }

    public long BankId { get; set; }

    public string ClientName { get; set; } = null!;

    public string RegisterationNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string VerificationStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<SalaryDisbursement> SalaryDisbursements { get; set; } = new List<SalaryDisbursement>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
