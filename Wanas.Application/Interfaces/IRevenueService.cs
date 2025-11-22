using Wanas.Application.DTOs.Revenue;

namespace Wanas.Application.Interfaces
{
 public interface IRevenueService
 {
 Task<RevenueSummaryDto> GetSummaryAsync(DateTime from, DateTime to);
 Task<IEnumerable<PaymentDto>> GetPaymentsAsync(DateTime from, DateTime to);
 }
}