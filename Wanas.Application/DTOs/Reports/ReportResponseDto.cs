using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reports
{
    public class ReportResponseDto
    {
        public int ReportId { get; set; }
        public string ReorterId { get; set; } 

        public ReportTarget TargetType { get; set; }
        public string TargetId { get; set; }    

        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } 


        public ReportStatus Status { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();



    }
}
