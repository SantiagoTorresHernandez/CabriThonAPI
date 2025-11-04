using CabriThonAPI.Application.DTOs.Metrics;

namespace CabriThonAPI.Application.Interfaces;

/// <summary>
/// Service interface for impact metrics
/// </summary>
public interface IMetricsService
{
    Task<ImpactMetricResponse> GetPromotionImpactAsync(string clientId, int year);
    Task<ImpactMetricResponse> GetOrderImpactAsync(string clientId, int year);
}

