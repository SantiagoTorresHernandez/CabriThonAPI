namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents monthly impact metrics for business improvements
/// </summary>
public class ImpactMetric : BaseEntity
{
    public Guid MetricId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public MetricType Type { get; set; }
    public decimal BenefitAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
}

/// <summary>
/// Type of metric impact
/// </summary>
public enum MetricType
{
    PromotionalSales = 0,
    StockoutPrevention = 1
}

