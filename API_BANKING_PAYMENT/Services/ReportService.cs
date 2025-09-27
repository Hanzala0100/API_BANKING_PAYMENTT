using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Enum;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using iTextSharp.text;
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

        // PDF styling constants
        private readonly BaseColor HEADER_BG_COLOR = new BaseColor(41, 128, 185);
        private readonly BaseColor ACCENT_COLOR = new BaseColor(52, 152, 219);
        private readonly BaseColor LIGHT_GRAY = new BaseColor(240, 240, 240);
        private readonly Font TITLE_FONT = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.WHITE);
        private readonly Font HEADER_FONT = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
        private readonly Font SUBHEADER_FONT = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.DARK_GRAY);
        private readonly Font NORMAL_FONT = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        private readonly Font BOLD_FONT = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

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
                    FileUrl = uploadResult.Data?.FileUrl,
                };

                var success = await _reportRepository.Add(report);
                if (!success)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("Failed to save report to database");

                // Retrieve the created report
                var userReports = await _reportRepository.GetReportsByUserIdAsync(user.UserId);
                var createdReport = userReports.OrderByDescending(r => r.GeneratedAt).FirstOrDefault();

                if (createdReport == null)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("Failed to retrieve created report");

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

                bool isAuthorized = user.Role switch
                {
                    Roles.SuperAdmin => true,
                    Roles.BankUser or Roles.ClientUser => report.GeneratedBy == user.UserId,
                    _ => false
                };

                if (!isAuthorized)
                    return BaseResponseDTO<ReportDTO>.ErrorResult("You are not authorized to access this report");

                var reportDTO = new ReportDTO
                {
                    ReportId = report.ReportId,
                    ReportType = report.ReportType,
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

                IEnumerable<Report> reports = user.Role switch
                {
                    Roles.ClientUser or Roles.BankUser => await _reportRepository.GetReportsByUserIdAsync(user.UserId),
                    Roles.SuperAdmin => await _reportRepository.GetAll(),
                    _ => Enumerable.Empty<Report>()
                };

                var reportDTOs = reports.Select(r => new ReportDTO
                {
                    ReportId = r.ReportId,
                    ReportType = r.ReportType,
                    GeneratedAt = r.GeneratedAt,
                    FileUrl = r.FileUrl ?? string.Empty,
                    GeneratedBy = r.GeneratedBy,
                    GeneratedByName = r.GeneratedByNavigation?.FullName ?? "Unknown"
                });

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

                if (user.Role != Roles.SuperAdmin && report.GeneratedBy != user.UserId)
                    return BaseResponseDTO<bool>.ErrorResult("You are not authorized to delete this report");

                var result = await _reportRepository.Delete(report);
                return result
                    ? BaseResponseDTO<bool>.SuccessResult(true, "Report deleted successfully")
                    : BaseResponseDTO<bool>.ErrorResult("Failed to delete report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting report with ID: {ReportId}", reportId);
                return BaseResponseDTO<bool>.ErrorResult("Error occurred while deleting report", new List<string> { ex.Message });
            }
        }

        // ================== ENHANCED PDF GENERATION METHODS ==================

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
                var pdfDocument = new iTextSharp.text.Document(PageSize.A4, 40, 40, 60, 40); // Explicit namespace
                var writer = PdfWriter.GetInstance(pdfDocument, stream);

                // Add header/footer
                writer.PageEvent = new PdfPageEventHelperz();

                pdfDocument.Open();

                // Title Section
                AddTitleSection(pdfDocument, $"CLIENT REPORT - {client.ClientName.ToUpper()}");

                // Client Information Table
                AddClientInfoTable(pdfDocument, client);

                // Summary Statistics
                AddSummarySection(pdfDocument, client);

                // Employees Section
                if (client.Employees.Any())
                    AddEmployeesTable(pdfDocument, client.Employees.ToList());

                // Beneficiaries Section
                if (client.Beneficiaries.Any())
                    AddBeneficiariesTable(pdfDocument, client.Beneficiaries.ToList());

                // Payments Section
                if (client.Payments.Any())
                    AddPaymentsTable(pdfDocument, client.Payments.ToList());

                // Salary Disbursements Section
                if (client.SalaryDisbursements.Any())
                    AddSalaryDisbursementsTable(pdfDocument, client.SalaryDisbursements.ToList(), client.Employees.ToList());

                pdfDocument.Close();
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
                    .ThenInclude(c => c.Payments)
                    .Include(b => b.Clients)
                    .ThenInclude(c => c.Employees)
                    .FirstOrDefaultAsync(b => b.BankId == bankId);

                if (bank == null) throw new Exception("Bank not found");

                using var stream = new MemoryStream();
                var pdfDocument = new iTextSharp.text.Document(PageSize.A4, 40, 40, 60, 40); // Explicit namespace
                var writer = PdfWriter.GetInstance(pdfDocument, stream);
                writer.PageEvent = new PdfPageEventHelperz();

                pdfDocument.Open();

                AddTitleSection(pdfDocument, $"BANK REPORT - {bank.BankName.ToUpper()}");
                AddBankInfoTable(pdfDocument, bank);
                AddBankClientsTable(pdfDocument, bank.Clients.ToList());
                AddBankStatistics(pdfDocument, bank);

                pdfDocument.Close();
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

                var totalBanks = await _context.Banks.CountAsync();
                var totalClients = await _context.Clients.CountAsync();
                var totalPayments = await _context.Payments.CountAsync();

                using var stream = new MemoryStream();
                var pdfDocument = new iTextSharp.text.Document(PageSize.A4, 40, 40, 60, 40); // Explicit namespace
                var writer = PdfWriter.GetInstance(pdfDocument, stream);
                writer.PageEvent = new PdfPageEventHelperz();

                pdfDocument.Open();

                AddTitleSection(pdfDocument, "SUPER ADMIN SYSTEM REPORT");
                AddSystemSummary(pdfDocument, totalBanks, totalClients, totalPayments);
                AddBankUsersTable(pdfDocument, bankUsers);

                pdfDocument.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating super admin report");
                throw;
            }
        }

        // ================== PDF HELPER METHODS ==================

        private void AddTitleSection(iTextSharp.text.Document document, string title)
        {
            var headerTable = new PdfPTable(1);
            headerTable.WidthPercentage = 100;
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            headerTable.DefaultCell.BackgroundColor = HEADER_BG_COLOR;
            headerTable.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
            headerTable.DefaultCell.Padding = 15;

            var titleParagraph = new Paragraph(title, TITLE_FONT);
            titleParagraph.SpacingAfter = 5f;
            headerTable.AddCell(titleParagraph);

            var dateParagraph = new Paragraph($"Generated on: {DateTime.UtcNow:MMMM dd, yyyy 'at' hh:mm tt} UTC",
                FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.WHITE));
            headerTable.AddCell(dateParagraph);

            document.Add(headerTable);
            document.Add(new Paragraph(" "));
        }

        private void AddClientInfoTable(iTextSharp.text.Document document, Client client)
        {
            var infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 30, 70 });
            infoTable.SpacingBefore = 10f;
            infoTable.SpacingAfter = 15f;

            AddInfoRow(infoTable, "Client Name:", client.ClientName);
            AddInfoRow(infoTable, "Registration No:", client.RegisterationNumber);
            AddInfoRow(infoTable, "Bank:", client.Bank?.BankName ?? "N/A");
            AddInfoRow(infoTable, "Address:", client.Address ?? "N/A");
            AddInfoRow(infoTable, "Verification Status:", client.VerificationStatus);
            AddInfoRow(infoTable, "Created Date:", client.CreatedAt.ToString("MMMM dd, yyyy"));

            document.Add(infoTable);
        }

        private void AddSummarySection(iTextSharp.text.Document document, Client client)
        {
            var summaryTable = new PdfPTable(4);
            summaryTable.WidthPercentage = 100;
            summaryTable.DefaultCell.Padding = 8;
            summaryTable.SpacingBefore = 10f;
            summaryTable.SpacingAfter = 20f;

            // Header
            AddSummaryHeaderCell(summaryTable, "Employees");
            AddSummaryHeaderCell(summaryTable, "Beneficiaries");
            AddSummaryHeaderCell(summaryTable, "Payments");
            AddSummaryHeaderCell(summaryTable, "Salary Disbursements");

            // Data
            AddSummaryDataCell(summaryTable, client.Employees.Count.ToString());
            AddSummaryDataCell(summaryTable, client.Beneficiaries.Count.ToString());
            AddSummaryDataCell(summaryTable, client.Payments.Count.ToString());
            AddSummaryDataCell(summaryTable, client.SalaryDisbursements.Count.ToString());

            document.Add(summaryTable);
        }

        private void AddEmployeesTable(iTextSharp.text.Document document, List<Employee> employees)
        {
            AddSectionHeader(document, "Employees");

            var table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 3, 3, 2, 2 });
            table.SpacingBefore = 10f;
            table.SpacingAfter = 15f;

            AddTableHeader(table, "ID");
            AddTableHeader(table, "Full Name");
            AddTableHeader(table, "Email");
            AddTableHeader(table, "Salary");
            AddTableHeader(table, "Status");

            foreach (var emp in employees)
            {
                AddTableCell(table, emp.EmployeeId.ToString(), NORMAL_FONT);
                AddTableCell(table, emp.FullName, NORMAL_FONT);
                AddTableCell(table, emp.Email, NORMAL_FONT);
                AddTableCell(table, emp.SalaryAmount.ToString("C"), NORMAL_FONT);
                AddTableCell(table, "Active", NORMAL_FONT); // Assuming all are active
            }

            document.Add(table);
        }

        private void AddBeneficiariesTable(iTextSharp.text.Document document, List<Beneficiary> beneficiaries)
        {
            AddSectionHeader(document, "Beneficiaries");

            var table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 3, 3, 2, 2 });
            table.SpacingBefore = 10f;

            AddTableHeader(table, "ID");
            AddTableHeader(table, "Full Name");
            AddTableHeader(table, "Account Number");
            AddTableHeader(table, "Bank Name");
            AddTableHeader(table, "IFSC Code");

            foreach (var ben in beneficiaries)
            {
                AddTableCell(table, ben.BeneficiaryId.ToString(), NORMAL_FONT);
                AddTableCell(table, ben.FullName, NORMAL_FONT);
                AddTableCell(table, ben.AccountNumber.ToString(), NORMAL_FONT);
                AddTableCell(table, ben.BankName, NORMAL_FONT);
                AddTableCell(table, ben.Ifsccode ?? "N/A", NORMAL_FONT);
            }

            document.Add(table);
        }

        private void AddPaymentsTable(iTextSharp.text.Document document, List<Payment> payments)
        {
            AddSectionHeader(document, "Payment History");

            var table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 2, 2, 2, 2 });
            table.SpacingBefore = 10f;

            AddTableHeader(table, "ID");
            AddTableHeader(table, "Amount");
            AddTableHeader(table, "Status");
            AddTableHeader(table, "Payment Date");
            AddTableHeader(table, "Beneficiary");

            foreach (var payment in payments)
            {
                AddTableCell(table, payment.PaymentId.ToString(), NORMAL_FONT);
                AddTableCell(table, payment.Amount.ToString("C"), NORMAL_FONT);
                AddTableCell(table, payment.Status, NORMAL_FONT);
                AddTableCell(table, payment.PaymentDate.ToString("MMM dd, yyyy"), NORMAL_FONT);
                AddTableCell(table, payment.Beneficiary?.FullName ?? "N/A", NORMAL_FONT);
            }

            document.Add(table);
        }

        private void AddSalaryDisbursementsTable(iTextSharp.text.Document document, List<SalaryDisbursement> disbursements, List<Employee> employees)
        {
            AddSectionHeader(document, "Salary Disbursements");

            var table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 3, 2, 2, 2 });
            table.SpacingBefore = 10f;

            AddTableHeader(table, "ID");
            AddTableHeader(table, "Employee");
            AddTableHeader(table, "Amount");
            AddTableHeader(table, "Status");
            AddTableHeader(table, "Disbursement Date");

            foreach (var disbursement in disbursements)
            {
                var employee = employees.FirstOrDefault(e => e.EmployeeId == disbursement.EmployeeId);
                AddTableCell(table, disbursement.SalaryId.ToString(), NORMAL_FONT);
                AddTableCell(table, employee?.FullName ?? "Unknown", NORMAL_FONT);
                AddTableCell(table, disbursement.Amount.ToString("C"), NORMAL_FONT);
                AddTableCell(table, disbursement.Status, NORMAL_FONT);
                AddTableCell(table, disbursement.DisbursementDate.ToString("MMM dd, yyyy"), NORMAL_FONT);
            }

            document.Add(table);
        }

        private void AddBankInfoTable(iTextSharp.text.Document document, Bank bank)
        {
            var infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 30, 70 });
            infoTable.SpacingBefore = 10f;
            infoTable.SpacingAfter = 15f;

            AddInfoRow(infoTable, "Bank Name:", bank.BankName);
            AddInfoRow(infoTable, "Address:", bank.Address);
            AddInfoRow(infoTable, "Contact Email:", bank.ContactEmail);
            AddInfoRow(infoTable, "Contact Phone:", bank.ContactPhone);
            AddInfoRow(infoTable, "Total Clients:", bank.Clients.Count.ToString());
            AddInfoRow(infoTable, "Established:", bank.CreatedAt.ToString("MMMM dd, yyyy"));

            document.Add(infoTable);
        }

        private void AddBankClientsTable(iTextSharp.text.Document document, List<Client> clients)
        {
            AddSectionHeader(document, "Bank Clients");

            var table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 3, 3, 2 });
            table.SpacingBefore = 10f;

            AddTableHeader(table, "ID");
            AddTableHeader(table, "Client Name");
            AddTableHeader(table, "Registration No");
            AddTableHeader(table, "Status");

            foreach (var client in clients)
            {
                AddTableCell(table, client.ClientId.ToString(), NORMAL_FONT);
                AddTableCell(table, client.ClientName, NORMAL_FONT);
                AddTableCell(table, client.RegisterationNumber, NORMAL_FONT);
                AddTableCell(table, client.VerificationStatus, NORMAL_FONT);
            }

            document.Add(table);
        }

        private void AddBankStatistics(iTextSharp.text.Document document, Bank bank)
        {
            var totalEmployees = bank.Clients.Sum(c => c.Employees.Count);
            var totalPayments = bank.Clients.Sum(c => c.Payments.Count);
            var totalSalaryDisbursements = bank.Clients.Sum(c => c.SalaryDisbursements.Count);

            var statsTable = new PdfPTable(3);
            statsTable.WidthPercentage = 100;
            statsTable.DefaultCell.Padding = 10;
            statsTable.SpacingBefore = 20f;

            AddSummaryHeaderCell(statsTable, "Total Employees");
            AddSummaryHeaderCell(statsTable, "Total Payments");
            AddSummaryHeaderCell(statsTable, "Salary Disbursements");

            AddSummaryDataCell(statsTable, totalEmployees.ToString());
            AddSummaryDataCell(statsTable, totalPayments.ToString());
            AddSummaryDataCell(statsTable, totalSalaryDisbursements.ToString());

            document.Add(statsTable);
        }

        private void AddSystemSummary(iTextSharp.text.Document document, int totalBanks, int totalClients, int totalPayments)
        {
            var statsTable = new PdfPTable(3);
            statsTable.WidthPercentage = 100;
            statsTable.DefaultCell.Padding = 12;
            statsTable.SpacingBefore = 10f;
            statsTable.SpacingAfter = 20f;

            AddSummaryHeaderCell(statsTable, "Total Banks");
            AddSummaryHeaderCell(statsTable, "Total Clients");
            AddSummaryHeaderCell(statsTable, "Total Payments");

            AddSummaryDataCell(statsTable, totalBanks.ToString());
            AddSummaryDataCell(statsTable, totalClients.ToString());
            AddSummaryDataCell(statsTable, totalPayments.ToString());

            document.Add(statsTable);
        }

        private void AddBankUsersTable(iTextSharp.text.Document document, List<User> bankUsers)
        {
            AddSectionHeader(document, "Bank Users");

            var table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 3, 3, 2, 2 });
            table.SpacingBefore = 10f;

            AddTableHeader(table, "ID");
            AddTableHeader(table, "Full Name");
            AddTableHeader(table, "Email");
            AddTableHeader(table, "Bank");
            AddTableHeader(table, "Created Date");

            foreach (var user in bankUsers)
            {
                AddTableCell(table, user.UserId.ToString(), NORMAL_FONT);
                AddTableCell(table, user.FullName, NORMAL_FONT);
                AddTableCell(table, user.Email, NORMAL_FONT);
                AddTableCell(table, user.Bank?.BankName ?? "N/A", NORMAL_FONT);
                AddTableCell(table, user.CreatedAt.ToString("MMM dd, yyyy"), NORMAL_FONT);
            }

            document.Add(table);
        }

        // ================== HELPER METHODS ==================

        private void AddInfoRow(PdfPTable table, string label, string value)
        {
            AddTableCell(table, label, BOLD_FONT, LIGHT_GRAY);
            AddTableCell(table, value ?? "N/A", NORMAL_FONT);
        }

        private void AddTableHeader(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, HEADER_FONT));
            cell.BackgroundColor = ACCENT_COLOR;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 8;
            table.AddCell(cell);
        }

        private void AddTableCell(PdfPTable table, string text, Font font, BaseColor bgColor = null)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = bgColor;
            cell.Padding = 6;
            cell.BorderWidth = 0.5f;
            table.AddCell(cell);
        }

        private void AddSectionHeader(iTextSharp.text.Document document, string title)
        {
            var header = new Paragraph(title, SUBHEADER_FONT);
            header.SpacingBefore = 20f;
            header.SpacingAfter = 10f;
            document.Add(header);
        }

        private void AddSummaryHeaderCell(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, BOLD_FONT));
            cell.BackgroundColor = ACCENT_COLOR;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 10;
            table.AddCell(cell);
        }

        private void AddSummaryDataCell(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, ACCENT_COLOR)));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 10;
            cell.BorderWidth = 1f;
            table.AddCell(cell);
        }

        // Page event helper for headers/footers
        private class PdfPageEventHelperz : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, iTextSharp.text.Document document)
            {
                var footer = new Paragraph($"Page {writer.PageNumber}",
                    FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY));
                footer.Alignment = Element.ALIGN_CENTER;

                var footerTable = new PdfPTable(1);
                footerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                footerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                footerTable.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                footerTable.AddCell(footer);
                footerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin, writer.DirectContent);
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