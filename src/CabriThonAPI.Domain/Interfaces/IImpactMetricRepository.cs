using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for impact metrics
/// </summary>
public interface IImpactMetricRepository
{
    Task<IEnumerable<ImpactMetric>> GetByClientIdAndYearAsync(long clientId, int year, MetricType? type = null);
    Task<decimal> GetTotalBenefitAsync(long clientId, int year, MetricType type);
}

