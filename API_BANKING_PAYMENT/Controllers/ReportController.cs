using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BANKING_PAYMENT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT required
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // ================= Generate & Upload Report =================
        [HttpPost("generate")]
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
                Data = result.Data
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
                    return NotFound(new
                    {
                        Success = false,
                        Message = result.Message
                    });

                if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("not authenticated", StringComparison.OrdinalIgnoreCase))
                    return Forbid(result.Message);

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

        // ================= Download PDF by ReportId =================
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadReport(long id)
        {
            var result = await _reportService.GetReportByIdAsync(id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new
                    {
                        Success = false,
                        Message = result.Message
                    });

                if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    return Forbid(result.Message);

                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message
                });
            }

            // Return the file URL for download
            return Ok(new
            {
                Success = true,
                Message = "Report download URL retrieved successfully",
                Data = result.Data
            });
        }

        // ================= Delete Report =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(long id)
        {
            var result = await _reportService.DeleteReportAsync(id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new
                    {
                        Success = false,
                        Message = result.Message
                    });

                if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                    return Forbid(result.Message);

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
    }
}