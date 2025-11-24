using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.DTOs.Reports;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;

        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitReport([FromBody] CreateReportDto dto)
        {
            //Jwt thing
            string? reporterId =User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(reporterId == null ) return Unauthorized("User is not Logged in");

            var result = await _reportService.SubmitReportAsync(dto, reporterId);

            return Ok(result);
        }


    }
}
