using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // ================= Generate & Upload Report  =================
        [HttpPost("generate-report")]
        public async Task<IActionResult> GenerateReport()
        {
            var result = await _reportService.GenerateAndUploadReportAsync();

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors
                });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                Data = result.Data,
            });
        }



        // ================= Get All Reports for Current User =================
        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var result = await _reportService.GetReportsForCurrentUserAsync();

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors
                });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
        }

        // ================= Get Report by Id =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(long id)
        {
            var result = await _reportService.GetReportByIdAsync(id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Success = false, Message = result.Message });

                if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { Success = false, Message = result.Message });

                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors
                });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
        }

        // ================= Download Report =================
        [HttpGet("download-report/{id:long}")]
        public async Task<IActionResult> DownloadReport(long id)
        {
            var result = await _reportService.GetReportByIdAsync(id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Success = false, Message = result.Message });

                if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { Success = false, Message = result.Message });

                return BadRequest(new { Success = false, Message = result.Message });
            }

            try
            {
                using var httpClient = new HttpClient();
                var pdfBytes = await httpClient.GetByteArrayAsync(result.Data.FileUrl);

                var fileName = $"{result.Data.ReportType}_Report_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName); 
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Report found but direct download failed",
                    Data = result.Data,
                    DownloadUrl = result.Data.FileUrl
                });
            }
        }


        // ================= Delete Report =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(long id)
        {
            var result = await _reportService.DeleteReportAsync(id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Success = false, Message = result.Message });

                if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { Success = false, Message = result.Message });

                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors
                });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
        }

        // ================= Get Report Statistics =================
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReportStatistics()
        {
            try
            {
                var reportsResult = await _reportService.GetReportsForCurrentUserAsync();

                if (!reportsResult.Success)
                    return BadRequest(new { Success = false, Message = reportsResult.Message });

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

                return Ok(new
                {
                    Success = true,
                    Message = "Report statistics retrieved successfully",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error retrieving report statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}