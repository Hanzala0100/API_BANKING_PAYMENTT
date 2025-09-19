using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models.Entities;

public partial class Bank
{
    public long BankId { get; set; }

    public string BankName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
