// Services/EmailTemplateService.cs
using API_BANKING_PAYMENT.Services.IServices;

namespace API_BANKING_PAYMENT.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(IConfiguration configuration, ILogger<EmailTemplateService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateClientVerificationEmail(string clientName, string status, string notes, Dictionary<string, string>? parameters = null)
        {
            try
            {
                var appName = _configuration["AppSettings:AppName"] ?? "Banking Payment System";
                var supportEmail = _configuration["AppSettings:SupportEmail"] ?? "support@bank.com";
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://yourapp.com";

                string subject = "";
                string body = "";

                switch (status.ToLower())
                {
                    case "approved":
                    case "verified":
                        subject = $"Congratulations! Your Business Account Has Been Approved - {appName}";
                        body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                                    .content {{ background: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
                                    .status-approved {{ color: #28a745; font-weight: bold; }}
                                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>
                                        <h1>Account Approved!</h1>
                                    </div>
                                    <div class='content'>
                                        <p>Dear {clientName},</p>
                                        <p>We are pleased to inform you that your business account has been <span class='status-approved'>approved</span> and is now fully active.</p>
                                        <p>You can now access all features of our payment system including:</p>
                                        <ul>
                                            <li>Employee management</li>
                                            <li>Beneficiary management</li>
                                            <li>Payment processing</li>
                                            <li>Transaction history</li>
                                        </ul>
                                        {(string.IsNullOrEmpty(notes) ? "" : $"<p><strong>Additional Notes:</strong> {notes}</p>")}
                                        <p>To get started, please log in to your account at: <a href='{baseUrl}/login'>{baseUrl}/login</a></p>
                                        <p>If you have any questions, please contact our support team at {supportEmail}.</p>
                                        <p>Best regards,<br/>The {appName} Team</p>
                                    </div>
                                    <div class='footer'>
                                        <p>This is an automated message. Please do not reply to this email.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";
                        break;

                    case "rejected":
                        subject = $"Update on Your Business Account Application - {appName}";
                        body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                    .header {{ background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                                    .content {{ background: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
                                    .status-rejected {{ color: #dc3545; font-weight: bold; }}
                                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>
                                        <h1>Application Review Complete</h1>
                                    </div>
                                    <div class='content'>
                                        <p>Dear {clientName},</p>
                                        <p>After careful review, we regret to inform you that your business account application has been <span class='status-rejected'>rejected</span>.</p>
                                        {(string.IsNullOrEmpty(notes) ? "<p>The application did not meet our current requirements.</p>" : $"<p><strong>Reason for Rejection:</strong> {notes}</p>")}
                                        <p>You may:</p>
                                        <ul>
                                            <li>Contact our support team for more information</li>
                                            <li>Submit additional documentation if applicable</li>
                                            <li>Reapply after addressing the concerns mentioned above</li>
                                        </ul>
                                        <p>If you believe this is an error or would like to discuss this further, please contact our support team at {supportEmail}.</p>
                                        <p>Best regards,<br/>The {appName} Team</p>
                                    </div>
                                    <div class='footer'>
                                        <p>This is an automated message. Please do not reply to this email.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";
                        break;

                    case "pending":
                    default:
                        subject = $"Additional Information Required for Your Application - {appName}";
                        body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                    .header {{ background: linear-gradient(135deg, #ffd89b 0%, #19547b 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                                    .content {{ background: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
                                    .status-pending {{ color: #ffc107; font-weight: bold; }}
                                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>
                                        <h1>Application Under Review</h1>
                                    </div>
                                    <div class='content'>
                                        <p>Dear {clientName},</p>
                                        <p>Your business account application is currently <span class='status-pending'>under review</span>.</p>
                                        {(string.IsNullOrEmpty(notes) ? "<p>Our team is currently reviewing your application and documents.</p>" : $"<p><strong>Additional Information:</strong> {notes}</p>")}
                                        <p>We will notify you once the review process is complete. This typically takes 2-3 business days.</p>
                                        <p>If we require any additional information, we will contact you via email.</p>
                                        <p>Thank you for your patience.</p>
                                        <p>Best regards,<br/>The {appName} Team</p>
                                    </div>
                                    <div class='footer'>
                                        <p>This is an automated message. Please do not reply to this email.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";
                        break;
                }

                // Apply custom parameters if provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        body = body.Replace($"{{{param.Key}}}", param.Value);
                    }
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email template for client: {ClientName}, status: {Status}", clientName, status);
                return $"<p>Dear {clientName},<br/>Your account status has been updated to: {status}.<br/>Notes: {notes}</p>";
            }
        }

        public string GenerateRejectionEmail(string clientName, string reasons, Dictionary<string, string>? parameters = null)
        {
            return GenerateClientVerificationEmail(clientName, "rejected", reasons, parameters);
        }

        public string GeneratePendingVerificationEmail(string clientName, Dictionary<string, string>? parameters = null)
        {
            try
            {
                var appName = _configuration["AppSettings:AppName"] ?? "Banking Payment System";
                var supportEmail = _configuration["AppSettings:SupportEmail"] ?? "support@bank.com";
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://yourapp.com";
                var documentUploadUrl = $"{baseUrl}/documents/upload";
                var dashboardUrl = $"{baseUrl}/dashboard";

                var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #ffd89b 0%, #19547b 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
                    .status-pending {{ color: #ff9800; font-weight: bold; background: #fff3cd; padding: 8px 15px; border-radius: 4px; display: inline-block; }}
                    .documents-section {{ background: #e8f4fd; padding: 20px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #2196F3; }}
                    .document-list {{ margin: 15px 0; }}
                    .document-item {{ margin: 8px 0; padding-left: 20px; position: relative; }}
                    .document-item:before {{ content: '•'; position: absolute; left: 8px; color: #2196F3; font-weight: bold; }}
                    .action-required {{ background: #fff3cd; padding: 15px; border-radius: 5px; border: 1px solid #ffc107; margin: 15px 0; }}
                    .button {{ display: inline-block; padding: 12px 24px; background: #2196F3; color: white; text-decoration: none; border-radius: 5px; margin: 10px 5px 10px 0; }}
                    .button-secondary {{ background: #6c757d; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    .note-box {{ background: #f8f9fa; padding: 15px; border-radius: 5px; border-left: 4px solid #6c757d; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Application Under Review</h1>
                        <p>Action Required: Document Submission</p>
                    </div>
                    <div class='content'>
                        <p>Dear {clientName},</p>
                        
                        <p>Thank you for submitting your business account application with {appName}. Your application is currently <span class='status-pending'>Under Review</span>.</p>
                        
                        <div class='action-required'>
                            <h3>📋 Action Required: Submit Required Documents</h3>
                            <p>To complete your verification process, please submit the following documents through our secure portal:</p>
                        </div>

                        <div class='documents-section'>
                            <h3>Required Documents for Verification:</h3>
                            <div class='document-list'>
                                <div class='document-item'><strong>Business License</strong> - Current business registration certificate</div>
                                <div class='document-item'><strong>Tax Identification Number (TIN) Certificate</strong></div>
                                <div class='document-item'><strong>Bank Statement</strong> - Last 3 months business bank statements</div>
                                <div class='document-item'><strong>Address Proof</strong> - Utility bill or rental agreement for business premises</div>
                                <div class='document-item'><strong>Identity Proof</strong> - Government-issued ID of authorized signatories</div>
                                <div class='document-item'><strong>KYC Documents</strong> - Know Your Customer compliance documents</div>
                            </div>
                        </div>

                        <div class='note-box'>
                            <h4>📝 Document Guidelines:</h4>
                            <ul>
                                <li>All documents must be clear, legible, and in PDF, JPEG, or PNG format</li>
                                <li>File size should not exceed 5MB per document</li>
                                <li>Documents should be current (not older than 3 months)</li>
                                <li>Ensure all information is visible and not cropped</li>
                            </ul>
                        </div>

                        <p><strong>Next Steps:</strong></p>
                        <ol>
                            <li>Gather all required documents listed above</li>
                            <li>Login to your account using the button below</li>
                            <li>Navigate to the 'Documents' section</li>
                            <li>Upload each document in the appropriate category</li>
                            <li>Submit for review</li>
                        </ol>

                        <div style='text-align: center; margin: 25px 0;'>
                            <a href='{documentUploadUrl}' class='button'>Upload Documents Now</a>
                            <a href='{dashboardUrl}' class='button button-secondary'>Go to Dashboard</a>
                        </div>

                        <p><strong>Important Notes:</strong></p>
                        <ul>
                            <li>Your application will remain in 'Pending' status until all required documents are submitted and verified</li>
                            <li>Verification typically takes 3-5 business days after all documents are received</li>
                            <li>You will receive email notifications at each stage of the verification process</li>
                            <li>Once verified, you will gain full access to all system features</li>
                        </ul>

                        <p>If you have already submitted these documents, please disregard this message. Our team will review them shortly.</p>

                        <p>If you encounter any issues during document upload or have questions about the required documents, please contact our support team at {supportEmail}.</p>

                        <p>We look forward to welcoming you to {appName}!</p>

                        <p>Best regards,<br/>
                        <strong>The {appName} Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message. Please do not reply to this email.</p>
                        <p>For security reasons, never share your login credentials with anyone.</p>
                    </div>
                </div>
            </body>
            </html>";

                // Apply custom parameters if provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        body = body.Replace($"{{{param.Key}}}", param.Value);
                    }
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pending verification email template for client: {ClientName}", clientName);
                return $@"
            <p>Dear {clientName},</p>
            <p>Your business account application is currently under review.</p>
            <p><strong>Action Required:</strong> Please submit the following documents for verification:</p>
            <ul>
                <li>Business License</li>
                <li>Tax Certificate</li>
                <li>Bank Statements</li>
                <li>Address Proof</li>
                <li>Identity Proof</li>
                <li>KYC Documents</li>
            </ul>
            <p>Please upload these documents through our secure portal.</p>
            <p>Best regards,<br/>{_configuration["AppSettings:AppName"] ?? "Banking Payment System"} Team</p>";
            }
        }
        public string GenerateClientUserWelcomeEmail(string fullName, string userName, string password, string email, Dictionary<string, string>? parameters = null)
        {
            try
            {
                var appName = _configuration["AppSettings:AppName"] ?? "Banking Payment System";
                var supportEmail = _configuration["AppSettings:SupportEmail"] ?? "support@bank.com";
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://yourapp.com";
                var loginUrl = $"{baseUrl}/login";

                var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
                    .credentials {{ background: #e9f7fe; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #2196F3; }}
                    .credential-item {{ margin: 10px 0; }}
                    .label {{ font-weight: bold; color: #555; }}
                    .value {{ color: #333; background: white; padding: 5px 10px; border-radius: 3px; display: inline-block; margin-left: 10px; }}
                    .security-note {{ background: #fff3cd; padding: 10px; border-radius: 5px; border-left: 4px solid #ffc107; margin: 15px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    .button {{ display: inline-block; padding: 12px 24px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to {appName}!</h1>
                    </div>
                    <div class='content'>
                        <p>Dear {fullName},</p>
                        
                        <p>Your client user account has been successfully created with {appName}. You can now access the client portal to manage your employees, beneficiaries, and payments.</p>
                        
                        <div class='credentials'>
                            <h3>Your Login Credentials:</h3>
                            <div class='credential-item'>
                                <span class='label'>Username:</span>
                                <span class='value'>{userName}</span>
                            </div>
                            <div class='credential-item'>
                                <span class='label'>Email:</span>
                                <span class='value'>{email}</span>
                            </div>
                            <div class='credential-item'>
                                <span class='label'>Password:</span>
                                <span class='value'>{password}</span>
                            </div>
                        </div>

                        <div class='security-note'>
                            <strong>Security Notice:</strong> For security reasons, we recommend that you change your password after your first login.
                        </div>

                        <p>To get started, please click the button below to access your account:</p>
                        
                        <div style='text-align: center;'>
                            <a href='{loginUrl}' class='button'>Login to Your Account</a>
                        </div>

                        <p>If the button doesn't work, you can copy and paste this link into your browser:<br/>
                        <a href='{loginUrl}'>{loginUrl}</a></p>

                        <p><strong>What you can do with your account:</strong></p>
                        <ul>
                            <li>Manage employee information</li>
                            <li>Add and manage beneficiaries</li>
                            <li>Process salary payments</li>
                            <li>View transaction history</li>
                            <li>Generate reports</li>
                        </ul>

                        <p>If you have any questions or need assistance, please contact our support team at {supportEmail}.</p>

                        <p>Best regards,<br/>
                        <strong>The {appName} Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message. Please do not reply to this email.</p>
                        <p>For security reasons, never share your login credentials with anyone.</p>
                    </div>
                </div>
            </body>
            </html>";

                // Apply custom parameters if provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        body = body.Replace($"{{{param.Key}}}", param.Value);
                    }
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating welcome email template for user: {UserName}", userName);
                return $@"
            <p>Dear {fullName},</p>
            <p>Your client user account has been created successfully.</p>
            <p><strong>Username:</strong> {userName}</p>
            <p><strong>Password:</strong> {password}</p>
            <p>Please login at: {parameters?["LoginUrl"] ?? "https://yourapp.com/login"}</p>
            <p>Best regards,<br/>Banking Payment System Team</p>";
            }
        }
    }
}
