using System;
using System.Collections.Generic;

namespace API_BANKING_PAYMENT.Models;

public partial class Report
{
    public long ReportId { get; set; }

    public long GeneratedBy { get; set; }

    public string ReportType { get; set; } = null!;

    public DateTime GeneratedAt { get; set; }

    public string? FileUrl { get; set; }

    public virtual User GeneratedByNavigation { get; set; } = null!;
}
