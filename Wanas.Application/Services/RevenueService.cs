using Wanas.Application.DTOs.Revenue;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
 public class RevenueService : IRevenueService
 {
 private readonly IUnitOfWork _uow;
 public RevenueService(IUnitOfWork uow) { _uow = uow; }
 public async Task<RevenueSummaryDto> GetSummaryAsync(DateTime from, DateTime to)
 {
 var payments = await _uow.Payments.FindAsync(p => p.PaymentDate >= from && p.PaymentDate <= to);
 var commissions = await _uow.Commissions.GetAllAsync();
 var relevantCommissions = commissions.Where(c => payments.Any(p => p.PaymentId == c.PaymentId));
 var gross = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
 var commissionAmt = relevantCommissions.Sum(c => c.PlatformAmount);
 var net = gross - commissionAmt;
 return new RevenueSummaryDto
 {
 From = from,
 To = to,
 GrossAmount = gross,
 CommissionAmount = commissionAmt,
 NetAmount = net,
 PaymentsCount = payments.Count(),
 FailedCount = payments.Count(p => p.Status == PaymentStatus.Failed),
 RefundedCount = payments.Count(p => p.Status == PaymentStatus.Refunded)
 };
 }
 public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync(DateTime from, DateTime to)
 {
 var payments = await _uow.Payments.FindAsync(p => p.PaymentDate >= from && p.PaymentDate <= to);
 return payments.Select(p => new PaymentDto
 {
 PaymentId = p.PaymentId,
 Amount = p.Amount,
 PaymentDate = p.PaymentDate,
 Status = p.Status.ToString(),
 UserId = p.UserId,
 ListingId = p.ListingId
 });
 }
 }
}