namespace API_BANKING_PAYMENT.Services.IServices
{
    public interface IEmailTemplateService
    {
        string GenerateClientVerificationEmail(string clientName, string status, string notes, Dictionary<string, string>? parameters = null);
        string GenerateRejectionEmail(string clientName, string reasons, Dictionary<string, string>? parameters = null);
        string GeneratePendingVerificationEmail(string clientName, Dictionary<string, string>? parameters = null);
        string GenerateClientUserWelcomeEmail(string fullName, string userName, string password, string email, Dictionary<string, string>? parameters = null);
    }
}