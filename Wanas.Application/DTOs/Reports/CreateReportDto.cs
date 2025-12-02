using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reports
{
    public class CreateReportDto
    {
        public ReportTarget TargetType { get; set; }

        public string TargetId { get; set; } = null!;

        public string Reason { get; set; } = null!;

        public ReportCategory Category { get; set; }

        public IFormFile[] Photos { get; set; }
    }
}
