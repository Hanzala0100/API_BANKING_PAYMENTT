using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_BANKING_PAYMENT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // ================= Generate & Upload Report  =================
        [HttpPost("generate-report")]
        public async Task<IActionResult> GenerateReport()
        {
            const string methodName = nameof(GenerateReport);

            _logger.LogInformation("{MethodName}: Starting report generation for user", methodName);

            try
            {
                var result = await _reportService.GenerateAndUploadReportAsync();

                if (!result.Success)
                {
                    _logger.LogWarning("{MethodName}: Report generation failed - {ErrorMessage}",
                        methodName, result.Message);

                    return BadRequest(BaseResponseDTO<object>.ErrorResult(
                        result.Message,
                        result.Errors
                    ));
                }

                _logger.LogInformation("{MethodName}: Report generated successfully. Report ID: {ReportId}",
                    methodName, result.Data?.ReportId);

                return Ok(BaseResponseDTO<object>.SuccessResult(
                    result.Data,
                    result.Message
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName}: Unexpected error during report generation", methodName);
                return BadRequest(BaseResponseDTO<object>.ErrorResult(
                    "An unexpected error occurred while generating the report",
                    new List<string> { ex.Message }
                ));
            }
        }

        // ================= Get All Reports for Current User =================
        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            const string methodName = nameof(GetMyReports);

            _logger.LogInformation("{MethodName}: Retrieving reports for current user", methodName);

            try
            {
                var result = await _reportService.GetReportsForCurrentUserAsync();

                if (!result.Success)
                {
                    _logger.LogWarning("{MethodName}: Failed to retrieve user reports - {ErrorMessage}",
                        methodName, result.Message);

                    return BadRequest(BaseResponseDTO<object>.ErrorResult(
                        result.Message,
                        result.Errors
                    ));
                }

                var reportCount = result.Data?.Count() ?? 0;
                _logger.LogInformation("{MethodName}: Successfully retrieved {ReportCount} reports for user",
                    methodName, reportCount);

                return Ok(BaseResponseDTO<object>.SuccessResult(
                    result.Data,
                    result.Message
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName}: Unexpected error while retrieving user reports", methodName);
                return BadRequest(BaseResponseDTO<object>.ErrorResult(
                    "An unexpected error occurred while retrieving reports",
                    new List<string> { ex.Message }
                ));
            }
        }

        // ================= Get Report by Id =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(long id)
        {
            const string methodName = nameof(GetReportById);

            _logger.LogInformation("{MethodName}: Retrieving report with ID: {ReportId}", methodName, id);

            try
            {
                var result = await _reportService.GetReportByIdAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("{MethodName}: Report not found - ID: {ReportId}", methodName, id);
                        return NotFound(BaseResponseDTO<object>.ErrorResult(result.Message));
                    }

                    if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("{MethodName}: Unauthorized access to report ID: {ReportId}",
                            methodName, id);
                        return Unauthorized(BaseResponseDTO<object>.ErrorResult(result.Message));
                    }

                    _logger.LogWarning("{MethodName}: Error retrieving report ID {ReportId} - {ErrorMessage}",
                        methodName, id, result.Message);

                    return BadRequest(BaseResponseDTO<object>.ErrorResult(
                        result.Message,
                        result.Errors
                    ));
                }

                _logger.LogInformation("{MethodName}: Successfully retrieved report ID: {ReportId}",
                    methodName, id);

                return Ok(BaseResponseDTO<object>.SuccessResult(
                    result.Data,
                    result.Message
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName}: Unexpected error while retrieving report ID: {ReportId}",
                    methodName, id);
                return BadRequest(BaseResponseDTO<object>.ErrorResult(
                    "An unexpected error occurred while retrieving the report",
                    new List<string> { ex.Message }
                ));
            }
        }

        // ================= Download Report =================
        [HttpGet("download-report/{id:long}")]
        public async Task<IActionResult> DownloadReport(long id)
        {
            const string methodName = nameof(DownloadReport);

            _logger.LogInformation("{MethodName}: Downloading report with ID: {ReportId}", methodName, id);

            try
            {
                var result = await _reportService.GetReportByIdAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("{MethodName}: Report not found for download - ID: {ReportId}",
                            methodName, id);
                        return NotFound(BaseResponseDTO<object>.ErrorResult(result.Message));
                    }

                    if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("{MethodName}: Unauthorized download attempt - Report ID: {ReportId}",
                            methodName, id);
                        return Unauthorized(BaseResponseDTO<object>.ErrorResult(result.Message));
                    }

                    _logger.LogWarning("{MethodName}: Error preparing download for report ID {ReportId} - {ErrorMessage}",
                        methodName, id, result.Message);

                    return BadRequest(BaseResponseDTO<object>.ErrorResult(result.Message));
                }

                _logger.LogInformation("{MethodName}: Attempting direct download for report ID: {ReportId}",
                    methodName, id);

                try
                {
                    using var httpClient = new HttpClient();
                    var pdfBytes = await httpClient.GetByteArrayAsync(result.Data.FileUrl);

                    _logger.LogInformation("{MethodName}: Direct download successful for report ID: {ReportId}",
                        methodName, id);

                    var fileName = $"{result.Data.ReportType}_Report_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                    return File(pdfBytes, "application/pdf", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{MethodName}: Direct download failed for report ID: {ReportId}, falling back to URL",
                        methodName, id);

                    return Ok(BaseResponseDTO<object>.SuccessResult(
                        new
                        {
                            result.Data,
                            DownloadUrl = result.Data.FileUrl,
                            DownloadError = ex.Message
                        },
                        "Report found but direct download failed"
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName}: Unexpected error during download process for report ID: {ReportId}",
                    methodName, id);
                return BadRequest(BaseResponseDTO<object>.ErrorResult(
                    "An unexpected error occurred during the download process",
                    new List<string> { ex.Message }
                ));
            }
        }

        // ================= Delete Report =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(long id)
        {
            const string methodName = nameof(DeleteReport);

            _logger.LogInformation("{MethodName}: Deleting report with ID: {ReportId}", methodName, id);

            try
            {
                var result = await _reportService.DeleteReportAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("{MethodName}: Report not found for deletion - ID: {ReportId}",
                            methodName, id);
                        return NotFound(BaseResponseDTO<object>.ErrorResult(result.Message));
                    }

                    if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("{MethodName}: Unauthorized deletion attempt - Report ID: {ReportId}",
                            methodName, id);
                        return Unauthorized(BaseResponseDTO<object>.ErrorResult(result.Message));
                    }

                    _logger.LogError("{MethodName}: Error deleting report ID {ReportId} - {ErrorMessage}",
                        methodName, id, result.Message);

                    return BadRequest(BaseResponseDTO<object>.ErrorResult(
                        result.Message,
                        result.Errors
                    ));
                }

                _logger.LogInformation("{MethodName}: Successfully deleted report ID: {ReportId}", methodName, id);

                return Ok(BaseResponseDTO<object>.SuccessResult(
                    result.Data,
                    result.Message
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName}: Unexpected error while deleting report ID: {ReportId}",
                    methodName, id);
                return BadRequest(BaseResponseDTO<object>.ErrorResult(
                    "An unexpected error occurred while deleting the report",
                    new List<string> { ex.Message }
                ));
            }
        }

        // ================= Get Report Statistics =================
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReportStatistics()
        {
            const string methodName = nameof(GetReportStatistics);

            _logger.LogInformation("{MethodName}: Retrieving report statistics for current user", methodName);

            try
            {
                var reportsResult = await _reportService.GetReportsForCurrentUserAsync();

                if (!reportsResult.Success)
                {
                    _logger.LogWarning("{MethodName}: Failed to retrieve reports for statistics - {ErrorMessage}",
                        methodName, reportsResult.Message);

                    return BadRequest(BaseResponseDTO<object>.ErrorResult(reportsResult.Message));
                }

                var reports = reportsResult.Data?.ToList() ?? new List<ReportDTO>();

                var statistics = new
                {
                    TotalReports = reports.Count,
                    ReportsByType = reports.GroupBy(r => r.ReportType)
                                          .ToDictionary(g => g.Key, g => g.Count()),
                    RecentReports = reports.OrderByDescending(r => r.GeneratedAt)
                                          .Take(5)
                                          .Select(r => new { r.ReportId, r.ReportType, r.GeneratedAt })
                };

                _logger.LogInformation(
                    "{MethodName}: Successfully generated statistics - Total reports: {TotalReports}, Types: {ReportTypes}",
                    methodName, statistics.TotalReports, string.Join(", ", statistics.ReportsByType.Keys));

                return Ok(BaseResponseDTO<object>.SuccessResult(
                    statistics,
                    "Report statistics retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName}: Unexpected error while generating report statistics", methodName);
                return BadRequest(BaseResponseDTO<object>.ErrorResult(
                    "Error retrieving report statistics",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}