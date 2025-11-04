using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for impact metrics
/// </summary>
public interface IImpactMetricRepository
{
    Task<IEnumerable<ImpactMetric>> GetByClientIdAndYearAsync(string clientId, int year, MetricType? type = null);
    Task<ImpactMetric?> GetByIdAsync(Guid metricId);
    Task<ImpactMetric> AddAsync(ImpactMetric metric);
    Task UpdateAsync(ImpactMetric metric);
    Task<decimal> GetTotalBenefitAsync(string clientId, int year, MetricType type);
}

