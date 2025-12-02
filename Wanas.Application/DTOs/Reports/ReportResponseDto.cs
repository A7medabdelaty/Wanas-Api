using System;
using System.Collections.Generic;
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reports
{
    public class ReportResponseDto
    {
        public int ReportId { get; set; }
        public string ReorterId { get; set; } 

        public ReportTarget TargetType { get; set; }
        public string TargetId { get; set; }
        public ReportCategory Category { get; set; } 

        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } 


        public ReportStatus Status { get; set; }
        public List<string>? PhotoUrls { get; set; } = new List<string>();

        // New fields for admin visibility
        public bool IsEscalated { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public string? EscalationReason { get; set; }
        public string? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }
        public ReportSeverity Severity { get; set; }
    }
}
