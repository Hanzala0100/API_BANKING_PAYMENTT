using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Enum;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using CloudinaryDotNet.Core;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API_BANKING_PAYMENT.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IDocumentService _documentService;
        private readonly IUserContext _userContext;
        private readonly BankDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IReportRepository reportRepository,
            IDocumentService documentService,
            IUserContext userContext,
            BankDbContext context,
            IMapper mapper,
            ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _documentService = documentService;
            _userContext = userContext;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseDTO<ReportDTO>> GenerateAndUploadReportAsync()
        {
            try
            {
                var user = _userContext.GetCurrentUser();
                if (user == null)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("User not authenticated");

                byte[] pdfBytes;
                string fileName;
                string docType = "Report";

                switch (user.Role)
                {
                    case Roles.ClientUser:
                        if (!user.ClientId.HasValue)
                            return BaseResponseDTO<ReportDTO>.ErrorResult("Client ID is required for ClientUser");
                        pdfBytes = await GenerateClientReportAsync(user.ClientId.Value);
                        fileName = $"ClientReport_{user.ClientId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                        break;
                    case Roles.BankUser:
                        if (!user.BankId.HasValue)
                            return BaseResponseDTO<ReportDTO>.ErrorResult("Bank ID is required for BankUser");
                        pdfBytes = await GenerateBankReportAsync(user.BankId.Value);
                        fileName = $"BankReport_{user.BankId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                        break;
                    case Roles.SuperAdmin:
                        pdfBytes = await GenerateSuperAdminReportAsync();
                        fileName = $"SuperAdminReport_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                        break;
                    default:
                        return BaseResponseDTO<ReportDTO>.ErrorResult("Role not allowed to generate reports");
                }

                // Upload PDF to document service
                using var stream = new MemoryStream(pdfBytes);
                var file = new FormFile(stream, 0, pdfBytes.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                };

                var uploadResult = await _documentService.UploadDocumentAsync(
                    file, user.UserId, user.BankId ?? 0, user.ClientId, docType);

                if (!uploadResult.Success)
                {
                    _logger.LogError("Failed to upload report for user {UserId}", user.UserId);
                    return BaseResponseDTO<ReportDTO>.ErrorResult("Failed to upload report document");
                }

                var report = new Report
                {
                    GeneratedBy = user.UserId,
                    GeneratedAt = DateTime.UtcNow,
                    ReportType = GetReportTypeByRole(user.Role),
                    FileUrl = uploadResult.Data?.FileUrl
                };

                // Since AddAsync returns bool, we need to save and then retrieve the report
                var success = await _reportRepository.Add(report);

                if (!success)
                {
                    return BaseResponseDTO<ReportDTO>.ErrorResult("Failed to save report to database");
                }

                // Now we need to retrieve the report to get the generated ID
                // You'll need a method to get the latest report by user ID
                var createdReport = await _reportRepository.GetById(report.ReportId);

                if (createdReport == null)
                {
                    return BaseResponseDTO<ReportDTO>.ErrorResult("Failed to retrieve created report");
                }

                // Manual mapping
                var reportDTO = new ReportDTO
                {
                    ReportId = createdReport.ReportId,
                    ReportType = createdReport.ReportType,
                    GeneratedAt = createdReport.GeneratedAt,
                    FileUrl = createdReport.FileUrl ?? string.Empty,
                    GeneratedBy = createdReport.GeneratedBy,
                    GeneratedByName = user.FullName
                };

                _logger.LogInformation("Report generated and uploaded successfully for user {UserId}", user.UserId);
                return BaseResponseDTO<ReportDTO>.SuccessResult(reportDTO, "Report generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating and uploading report");
                return BaseResponseDTO<ReportDTO>.ErrorResult("Error occurred while generating report", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<ReportDTO>> GetReportByIdAsync(long reportId)
        {
            try
            {
                var report = await _reportRepository.GetById(reportId);
                if (report == null)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("Report not found");

                var user = _userContext.GetCurrentUser();
                if (user == null)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("User not authenticated");

                // Authorization check
                bool isAuthorized = user.Role switch
                {
                    Roles.SuperAdmin => true,
                    Roles.BankUser or Roles.ClientUser => report.GeneratedBy == user.UserId,
                    _ => false
                };

                if (!isAuthorized)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("You are not authorized to access this report");

                // Simple mapping - no more enum conversion needed!
                var reportDTO = new ReportDTO
                {
                    ReportId = report.ReportId,
                    ReportType = report.ReportType, // Direct assignment - both are strings now
                    GeneratedAt = report.GeneratedAt,
                    FileUrl = report.FileUrl ?? string.Empty,
                    GeneratedBy = report.GeneratedBy,
                    GeneratedByName = report.GeneratedByNavigation?.FullName ?? "Unknown"
                };

                return BaseResponseDTO<ReportDTO>.SuccessResult(reportDTO, "Report retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving report with ID: {ReportId}", reportId);
                return BaseResponseDTO<ReportDTO>.ErrorResult("Error occurred while retrieving report", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<ReportDTO>>> GetReportsForCurrentUserAsync()
        {
            try
            {
                var user = _userContext.GetCurrentUser();
                if (user == null)
                    return BaseResponseDTO<IEnumerable<ReportDTO>>.ErrorResult("User not authenticated");

                IEnumerable<Report> reports;

                switch (user.Role)
                {
                    case Roles.ClientUser:
                    case Roles.BankUser:
                        reports = await _reportRepository.GetReportsByUserIdAsync(user.UserId);
                        break;
                    case Roles.SuperAdmin:
                        reports = await _reportRepository.GetAll();
                        break;
                    default:
                        reports = Enumerable.Empty<Report>();
                        break;
                }

                var reportDTOs = _mapper.Map<IEnumerable<ReportDTO>>(reports);
                return BaseResponseDTO<IEnumerable<ReportDTO>>.SuccessResult(reportDTOs, "Reports retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving reports for current user");
                return BaseResponseDTO<IEnumerable<ReportDTO>>.ErrorResult("Error occurred while retrieving reports", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteReportAsync(long reportId)
        {
            try
            {
                var report = await _reportRepository.GetById(reportId);
                if (report == null)
                    return BaseResponseDTO<bool>.ErrorResult("Report not found");

                var user = _userContext.GetCurrentUser();
                if (user == null)
                    return BaseResponseDTO<bool>.ErrorResult("User not authenticated");

                // Authorization: Only allow users to delete their own reports, or SuperAdmin
                if (user.Role != Roles.SuperAdmin && report.GeneratedBy != user.UserId)
                    return BaseResponseDTO<bool>.ErrorResult("You are not authorized to delete this report");

                var reportdel = await _reportRepository.GetById(reportId);
                var result = await _reportRepository.Delete(reportdel);
                if (result)
                    return BaseResponseDTO<bool>.SuccessResult(true, "Report deleted successfully");
                else
                    return BaseResponseDTO<bool>.ErrorResult("Failed to delete report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting report with ID: {ReportId}", reportId);
                return BaseResponseDTO<bool>.ErrorResult("Error occurred while deleting report", new List<string> { ex.Message });
            }
        }

        // ================== PDF Generation Methods (Keep these as-is) ==================

        public async Task<byte[]> GenerateClientReportAsync(long clientId)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Employees)
                    .Include(c => c.Beneficiaries)
                    .Include(c => c.Payments)
                    .Include(c => c.SalaryDisbursements)
                    .Include(c => c.Bank)
                    .FirstOrDefaultAsync(c => c.ClientId == clientId);

                if (client == null) throw new Exception("Client not found");

                using var stream = new MemoryStream();
                var pdf = new iTextSharp.text.Document();
                PdfWriter.GetInstance(pdf, stream);
                pdf.Open();

                pdf.Add(new iTextSharp.text.Paragraph($"Client Report - {client.ClientName}"));
                pdf.Add(new iTextSharp.text.Paragraph($"Registration No: {client.RegisterationNumber}"));
                pdf.Add(new iTextSharp.text.Paragraph($"Bank: {client.Bank.BankName}"));
                pdf.Add(new iTextSharp.text.Paragraph($"Created At: {client.CreatedAt}"));
                pdf.Add(new iTextSharp.text.Paragraph(" "));

                // Employees Table
                pdf.Add(new iTextSharp.text.Paragraph("Employees"));
                var empTable = new PdfPTable(4);
                empTable.AddCell("ID"); empTable.AddCell("FullName"); empTable.AddCell("Email"); empTable.AddCell("Salary");
                foreach (var e in client.Employees)
                {
                    empTable.AddCell(e.EmployeeId.ToString());
                    empTable.AddCell(e.FullName);
                    empTable.AddCell(e.Email);
                    empTable.AddCell(e.SalaryAmount.ToString("C"));
                }
                pdf.Add(empTable);
                pdf.Add(new iTextSharp.text.Paragraph(" "));

                // Beneficiaries Table
                pdf.Add(new iTextSharp.text.Paragraph("Beneficiaries"));
                var benTable = new PdfPTable(4);
                benTable.AddCell("ID"); benTable.AddCell("FullName"); benTable.AddCell("AccountNumber"); benTable.AddCell("BankName");
                foreach (var b in client.Beneficiaries)
                {
                    benTable.AddCell(b.BeneficiaryId.ToString());
                    benTable.AddCell(b.FullName);
                    benTable.AddCell(b.AccountNumber.ToString());
                    benTable.AddCell(b.BankName);
                }
                pdf.Add(benTable);
                pdf.Add(new iTextSharp.text.Paragraph(" "));

                // Payments Table
                pdf.Add(new iTextSharp.text.Paragraph("Payments"));
                var payTable = new PdfPTable(4);
                payTable.AddCell("ID"); payTable.AddCell("Amount"); payTable.AddCell("Status"); payTable.AddCell("Date");
                foreach (var p in client.Payments)
                {
                    payTable.AddCell(p.PaymentId.ToString());
                    payTable.AddCell(p.Amount.ToString("C"));
                    payTable.AddCell(p.Status);
                    payTable.AddCell(p.PaymentDate.ToShortDateString());
                }
                pdf.Add(payTable);
                pdf.Add(new iTextSharp.text.Paragraph(" "));

                // Salary Disbursements Table
                pdf.Add(new iTextSharp.text.Paragraph("Salary Disbursements"));
                var salTable = new PdfPTable(5);
                salTable.AddCell("ID"); salTable.AddCell("Employee"); salTable.AddCell("Amount"); salTable.AddCell("Status"); salTable.AddCell("Date");
                foreach (var s in client.SalaryDisbursements)
                {
                    salTable.AddCell(s.SalaryId.ToString());
                    salTable.AddCell(client.Employees.First(e => e.EmployeeId == s.EmployeeId).FullName);
                    salTable.AddCell(s.Amount.ToString("C"));
                    salTable.AddCell(s.Status);
                    salTable.AddCell(s.DisbursementDate.ToShortDateString());
                }
                pdf.Add(salTable);

                pdf.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating client report for ClientId {ClientId}", clientId);
                throw;
            }
        }

        public async Task<byte[]> GenerateBankReportAsync(long bankId)
        {
            try
            {
                var bank = await _context.Banks
                    .Include(b => b.Clients)
                    .FirstOrDefaultAsync(b => b.BankId == bankId);

                if (bank == null) throw new Exception("Bank not found");

                using var stream = new MemoryStream();
                var pdf = new iTextSharp.text.Document();
                PdfWriter.GetInstance(pdf, stream);
                pdf.Open();

                pdf.Add(new iTextSharp.text.Paragraph($"Bank Report - {bank.BankName}"));
                pdf.Add(new iTextSharp.text.Paragraph($"Address: {bank.Address}"));
                pdf.Add(new iTextSharp.text.Paragraph($"Email: {bank.ContactEmail}, Phone: {bank.ContactPhone}"));
                pdf.Add(new iTextSharp.text.Paragraph($"Created At: {bank.CreatedAt}"));
                pdf.Add(new iTextSharp.text.Paragraph(" "));

                // Clients Table (basic info)
                pdf.Add(new iTextSharp.text.Paragraph("Clients"));
                var clientTable = new PdfPTable(3);
                clientTable.AddCell("ID"); clientTable.AddCell("Name"); clientTable.AddCell("Registration No");
                foreach (var c in bank.Clients)
                {
                    clientTable.AddCell(c.ClientId.ToString());
                    clientTable.AddCell(c.ClientName);
                    clientTable.AddCell(c.RegisterationNumber);
                }
                pdf.Add(clientTable);
                pdf.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bank report for BankId {BankId}", bankId);
                throw;
            }
        }

        public async Task<byte[]> GenerateSuperAdminReportAsync()
        {
            try
            {
                var bankUsers = await _context.Users
                    .Where(u => u.Role == Roles.BankUser)
                    .Include(u => u.Bank)
                    .ToListAsync();

                using var stream = new MemoryStream();
                var pdf = new iTextSharp.text.Document();
                PdfWriter.GetInstance(pdf, stream);
                pdf.Open();

                pdf.Add(new iTextSharp.text.Paragraph("SuperAdmin Report - Bank Users"));
                pdf.Add(new iTextSharp.text.Paragraph($"Generated At: {DateTime.UtcNow}"));
                pdf.Add(new iTextSharp.text.Paragraph(" "));

                var table = new PdfPTable(5);
                table.AddCell("User ID"); table.AddCell("Full Name"); table.AddCell("Email"); table.AddCell("Bank Name"); table.AddCell("Created At");
                foreach (var u in bankUsers)
                {
                    table.AddCell(u.UserId.ToString());
                    table.AddCell(u.FullName);
                    table.AddCell(u.Email);
                    table.AddCell(u.Bank?.BankName ?? "-");
                    table.AddCell(u.CreatedAt.ToShortDateString());
                }

                pdf.Add(table);
                pdf.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating super admin report");
                throw;
            }
        }

        private string GetReportTypeByRole(string role)
        {
            return role switch
            {
                Roles.ClientUser => ReportType.Client,
                Roles.BankUser => ReportType.Bank,
                Roles.SuperAdmin => ReportType.SuperAdmin,
                _ => "Other"
            };
        }
    }
}