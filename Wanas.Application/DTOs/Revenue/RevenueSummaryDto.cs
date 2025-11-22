namespace Wanas.Application.DTOs.Revenue
{
 public class RevenueSummaryDto
 {
 public DateTime From { get; set; }
 public DateTime To { get; set; }
 public decimal GrossAmount { get; set; }
 public decimal CommissionAmount { get; set; }
 public decimal NetAmount { get; set; }
 public int PaymentsCount { get; set; }
 public int FailedCount { get; set; }
 public int RefundedCount { get; set; }
 }
}