namespace API_BANKING_PAYMENT.Models.Enum
{
    public static class VerificationStatus
    {
        public const string Pending = "Pending";
        public const string InReview = "InReview";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";
        public const string Suspended = "Suspended";

        public static readonly Dictionary<string, string[]> ValidTransitions = new()
        {
            [Pending] = new[] { InReview, Verified, Rejected },
            [InReview] = new[] { Verified, Rejected, Pending },
            [Verified] = new[] { Suspended, InReview },
            [Rejected] = new[] { InReview, Pending },
            [Suspended] = new[] { Verified, Rejected }
        };

        public static bool IsValidTransition(string currentStatus, string newStatus)
        {
            return ValidTransitions.ContainsKey(currentStatus) &&
                   ValidTransitions[currentStatus].Contains(newStatus);
        }

        public static string[] GetAllStatuses() => new[] { Pending, InReview, Verified, Rejected, Suspended };
    }
}
