namespace Wanas.Application.DTOs.Revenue
{
 public class PaymentDto
 {
 public int PaymentId { get; set; }
 public decimal Amount { get; set; }
 public DateTime PaymentDate { get; set; }
 public string Status { get; set; } = string.Empty;
 public string UserId { get; set; } = string.Empty;
 public int ListingId { get; set; }
 }
}