using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models.Entities;

public partial class User
{
    public long UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public long? BankId { get; set; }

    public long? ClientId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Bank? Bank { get; set; }

    public virtual Client? Client { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
