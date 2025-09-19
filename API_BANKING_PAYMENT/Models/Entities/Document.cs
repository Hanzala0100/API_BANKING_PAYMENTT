using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models.Entities;

public partial class Document
{
    public long DocumentId { get; set; }

    public long UploadedBy { get; set; }

    public long? ClientId { get; set; }

    public long BankId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? DocType { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual User UploadedByNavigation { get; set; } = null!;
}
