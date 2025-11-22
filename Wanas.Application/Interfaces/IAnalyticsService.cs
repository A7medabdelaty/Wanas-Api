using Wanas.Application.DTOs.Analytics;

namespace Wanas.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsSummaryDto> GetSummaryAsync(DateOnly date);
        Task<IEnumerable<TrafficPointDto>> GetTrafficAsync(DateOnly from, DateOnly to);
        Task<ModerationKpiDto> GetModerationKpisAsync(DateOnly date);
    }
}