namespace Wanas.Application.DTOs.Reports
{
    public class EscalateReportDto
    {
        public string Reason { get; set; } = null!;
        public bool CancelEscalation { get; set; } = false; // allow admin to de-escalate
    }
}