using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace API_BANKING_PAYMENT.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _usersRepository;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IEmailTemplateService emailTemplateService,
            IClientRepository clientRepository,
            ILogger<EmailService> logger,
            IUserRepository usersRepository)
        {
            _configuration = configuration;
            _emailTemplateService = emailTemplateService;
            _clientRepository = clientRepository;
            _logger = logger;
            _usersRepository = usersRepository;
        }

        public async Task<BaseResponseDTO<bool>> SendEmailAsync(EmailRequestDTO emailRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(emailRequest.ToEmail))
                    return BaseResponseDTO<bool>.ErrorResult("Recipient email is required");

                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var fromEmail = smtpSettings["FromEmail"] ?? "noreply@bank.com";
                var fromName = smtpSettings["FromName"] ?? "Banking Payment System";

                using (var client = CreateSmtpClient())
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromEmail, fromName);
                    message.To.Add(emailRequest.ToEmail);
                    message.Subject = emailRequest.Subject;
                    message.Body = emailRequest.Body;
                    message.IsBodyHtml = emailRequest.IsHtml;

                    // Add CC emails if any
                    if (emailRequest.CcEmails != null && emailRequest.CcEmails.Any())
                    {
                        foreach (var ccEmail in emailRequest.CcEmails)
                        {
                            if (!string.IsNullOrEmpty(ccEmail))
                                message.CC.Add(ccEmail);
                        }
                    }

                    // Add BCC emails if any
                    if (emailRequest.BccEmails != null && emailRequest.BccEmails.Any())
                    {
                        foreach (var bccEmail in emailRequest.BccEmails)
                        {
                            if (!string.IsNullOrEmpty(bccEmail))
                                message.Bcc.Add(bccEmail);
                        }
                    }

                    await client.SendMailAsync(message);
                }

                _logger.LogInformation("Email sent successfully to: {ToEmail}, Subject: {Subject}", emailRequest.ToEmail, emailRequest.Subject);
                return BaseResponseDTO<bool>.SuccessResult(true, "Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to: {ToEmail}", emailRequest.ToEmail);
                return BaseResponseDTO<bool>.ErrorResult("Error sending email", new List<string> { ex.Message });
            }
        }
        public async Task<BaseResponseDTO<bool>> SendClientUserWelcomeEmailAsync(ClientUserCreationDTO clientUser)
        {
            try
            {
                var emailBody = _emailTemplateService.GenerateClientUserWelcomeEmail(
                    clientUser.FullName,
                    clientUser.UserName,
                    clientUser.Password,
                    clientUser.Email,
                    new Dictionary<string, string>
                    {
                        ["LoginUrl"] = _configuration["AppSettings:BaseUrl"] + "/login",
                        ["SupportEmail"] = _configuration["AppSettings:SupportEmail"] ?? "support@bank.com",
                        ["AppName"] = _configuration["AppSettings:AppName"] ?? "Banking Payment System"
                    }
                );

                var emailRequest = new EmailRequestDTO
                {
                    ToEmail = clientUser.Email,
                    Subject = $"Welcome to {_configuration["AppSettings:AppName"] ?? "Banking Payment System"} - Your Account Details",
                    Body = emailBody,
                    IsHtml = true
                };

                var result = await SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Welcome email sent successfully to client user: {Email}", clientUser.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to send welcome email to client user: {Email}. Error: {Error}",
                        clientUser.Email, result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to client user: {Email}", clientUser.Email);
                return BaseResponseDTO<bool>.ErrorResult("Error sending welcome email", new List<string> { ex.Message });
            }
        }
        public async Task<BaseResponseDTO<bool>> SendPendingVerificationEmailAsync(long clientId, string userEmail)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<bool>.ErrorResult("Client not found");

                var emailBody = _emailTemplateService.GeneratePendingVerificationEmail(
                    client.ClientName,
                    new Dictionary<string, string>
                    {
                        ["LoginUrl"] = _configuration["AppSettings:BaseUrl"] + "/login",
                        ["DocumentUploadUrl"] = _configuration["AppSettings:BaseUrl"] + "/documents/upload",
                        ["DashboardUrl"] = _configuration["AppSettings:BaseUrl"] + "/dashboard",
                        ["SupportEmail"] = _configuration["AppSettings:SupportEmail"] ?? "support@bank.com",
                        ["AppName"] = _configuration["AppSettings:AppName"] ?? "Banking Payment System",
                        ["RegistrationNumber"] = client.RegisterationNumber,
                        ["DeadlineDate"] = DateTime.UtcNow.AddDays(7).ToString("MMMM dd, yyyy"),
                        ["ClientId"] = client.ClientId.ToString()
                    }
                );

                var emailRequest = new EmailRequestDTO
                {
                    ToEmail = userEmail,
                    Subject = $"Action Required: Document Submission for {client.ClientName} - {_configuration["AppSettings:AppName"] ?? "Banking Payment System"}",
                    Body = emailBody,
                    IsHtml = true
                };

                var result = await SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Pending verification email sent successfully to: {Email} for client: {ClientName}",
                        userEmail, client.ClientName);
                }
                else
                {
                    _logger.LogWarning("Failed to send pending verification email to: {Email}. Error: {Error}",
                        userEmail, result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pending verification email to: {Email} for client ID: {ClientId}",
                    userEmail, clientId);
                return BaseResponseDTO<bool>.ErrorResult("Error sending pending verification email", new List<string> { ex.Message });
            }
        }
        public async Task<BaseResponseDTO<bool>> SendApprovalEmailAsync(long clientId, string notes)
        {
            try
            {
                var clientEmailRequest = new ClientEmailRequestDTO
                {
                    ClientId = clientId,
                    EmailType = "approved",
                    Parameters = new Dictionary<string, string>
                    {
                        ["Notes"] = notes,
                        ["VerificationDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
                    }
                };

                return await SendClientVerificationEmailAsync(clientEmailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending approval email for client ID: {ClientId}", clientId);
                return BaseResponseDTO<bool>.ErrorResult("Error sending approval email", new List<string> { ex.Message });
            }
        }
        public async Task<BaseResponseDTO<bool>> SendRejectionEmailAsync(long clientId, string reasons)
        {
            try
            {
                var clientEmailRequest = new ClientEmailRequestDTO
                {
                    ClientId = clientId,
                    EmailType = "rejected",
                    Parameters = new Dictionary<string, string>
                    {
                        ["Reasons"] = reasons,
                        ["RejectionDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
                    }
                };

                return await SendClientVerificationEmailAsync(clientEmailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending rejection email for client ID: {ClientId}", clientId);
                return BaseResponseDTO<bool>.ErrorResult("Error sending rejection email", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> SendClientVerificationEmailAsync(ClientEmailRequestDTO clientEmailRequest)
        {
            try
            {
                var client = await _clientRepository.GetById(clientEmailRequest.ClientId);
                if (client == null)
                    return BaseResponseDTO<bool>.ErrorResult("Client not found");

                var clientUsers = await _usersRepository.GetUsersByClientId(clientEmailRequest.ClientId);
                if (clientUsers == null || !clientUsers.Any())
                    return BaseResponseDTO<bool>.ErrorResult("No users found for this client");

                var validUsers = clientUsers.Where(u => !string.IsNullOrEmpty(u.Email)).ToList();
                if (!validUsers.Any())
                    return BaseResponseDTO<bool>.ErrorResult("No valid email addresses found for client users");

                string subject = "";
                string body = "";

                switch (clientEmailRequest.EmailType.ToLower())
                {
                    case "approved":
                    case "verified":
                        body = _emailTemplateService.GenerateClientVerificationEmail(
                            client.ClientName,
                            "approved",
                            clientEmailRequest.Parameters.ContainsKey("Notes") ? clientEmailRequest.Parameters["Notes"] : "",
                            clientEmailRequest.Parameters);
                        subject = $"Your Business Account Has Been Approved - {_configuration["AppSettings:AppName"] ?? "Banking Payment System"}";
                        break;

                    case "rejected":
                        body = _emailTemplateService.GenerateRejectionEmail(
                            client.ClientName,
                            clientEmailRequest.Parameters.ContainsKey("Reasons") ? clientEmailRequest.Parameters["Reasons"] : "",
                            clientEmailRequest.Parameters);
                        subject = $"Update on Your Business Account Application - {_configuration["AppSettings:AppName"] ?? "Banking Payment System"}";
                        break;

                    case "pending":
                        body = _emailTemplateService.GeneratePendingVerificationEmail(
                            client.ClientName,
                            clientEmailRequest.Parameters);
                        subject = $"Additional Information Required for Your Application - {_configuration["AppSettings:AppName"] ?? "Banking Payment System"}";
                        break;

                    default:
                        return BaseResponseDTO<bool>.ErrorResult($"Unsupported email type: {clientEmailRequest.EmailType}");
                }

                var sendTasks = validUsers.Select(user => SendEmailToUserAsync(user, subject, body));
                var results = await Task.WhenAll(sendTasks);

                var successCount = results.Count(r => r.Success);
                var failedCount = results.Count(r => !r.Success);

                if (failedCount == 0)
                {
                    _logger.LogInformation("Client verification email sent successfully to all {Count} users for client: {ClientName}",
                        successCount, client.ClientName);
                    return BaseResponseDTO<bool>.SuccessResult(true, $"Email sent to {successCount} users");
                }
                else
                {
                    _logger.LogWarning("Client verification email partially sent. Success: {SuccessCount}, Failed: {FailedCount} for client: {ClientName}",
                        successCount, failedCount, client.ClientName);
                    return BaseResponseDTO<bool>.SuccessResult(true,
                        $"Email sent to {successCount} users, failed for {failedCount} users");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending client verification email for client ID: {ClientId}", clientEmailRequest.ClientId);
                return BaseResponseDTO<bool>.ErrorResult("Error sending client verification email", new List<string> { ex.Message });
            }
        }

        private async Task<BaseResponseDTO<bool>> SendEmailToUserAsync(User user, string subject, string body)
        {
            try
            {
                var emailRequest = new EmailRequestDTO
                {
                    ToEmail = user.Email,
                    Subject = subject,
                    Body = body,
                    IsHtml = true
                };

                var result = await SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Email sent successfully to user: {UserName} ({Email})",
                        user.FullName, user.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to send email to user: {UserName} ({Email}). Error: {Error}",
                        user.FullName, user.Email, result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to user: {UserName} ({Email})", user.FullName, user.Email);
                return BaseResponseDTO<bool>.ErrorResult($"Failed to send email to {user.Email}");
            }
        }
        public async Task<BaseResponseDTO<bool>> SendBulkEmailsAsync(List<EmailRequestDTO> emailRequests)
        {
            try
            {
                var tasks = emailRequests.Select(emailRequest => SendEmailAsync(emailRequest));
                var results = await Task.WhenAll(tasks);

                var successful = results.Count(r => r.Success);
                var failed = results.Count(r => !r.Success);

                _logger.LogInformation("Bulk email sending completed. Successful: {Successful}, Failed: {Failed}", successful, failed);

                if (failed == 0)
                    return BaseResponseDTO<bool>.SuccessResult(true, $"All {successful} emails sent successfully");
                else
                    return BaseResponseDTO<bool>.SuccessResult(true, $"{successful} emails sent successfully, {failed} failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk emails");
                return BaseResponseDTO<bool>.ErrorResult("Error sending bulk emails", new List<string> { ex.Message });
            }
        }
        private SmtpClient CreateSmtpClient()
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");

            return new SmtpClient
            {
                Host = smtpSettings["Host"] ?? "smtp.gmail.com",
                Port = int.Parse(smtpSettings["Port"] ?? "587"),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true"),
                Credentials = new NetworkCredential(
                    smtpSettings["Username"],
                    smtpSettings["Password"]
                ),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };
        }
    }
}