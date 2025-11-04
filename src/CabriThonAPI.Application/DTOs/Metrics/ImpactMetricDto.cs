namespace CabriThonAPI.Application.DTOs.Metrics;

public class MonthlyImpactDto
{
    public int Month { get; set; }
    public decimal BenefitAmount { get; set; }
    public int ItemsCount { get; set; }
}

public class ImpactMetricResponse
{
    public int Year { get; set; }
    public decimal TotalBenefit { get; set; }
    public List<MonthlyImpactDto> MonthlyBreakdown { get; set; } = new();
}

