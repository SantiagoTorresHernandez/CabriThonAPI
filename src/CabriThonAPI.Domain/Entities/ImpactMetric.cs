namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents monthly impact metrics for business improvements
/// This is a virtual entity used for analytics, not directly mapped to a table
/// </summary>
public class ImpactMetric
{
    public long ClientId { get; set; }
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

