using CabriThonAPI.Application.DTOs.Metrics;
using CabriThonAPI.Application.Interfaces;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;

namespace CabriThonAPI.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly IImpactMetricRepository _metricRepository;

    public MetricsService(IImpactMetricRepository metricRepository)
    {
        _metricRepository = metricRepository;
    }

    public async Task<ImpactMetricResponse> GetPromotionImpactAsync(string clientId, int year)
    {
        if (!long.TryParse(clientId, out var clientIdLong))
        {
            throw new ArgumentException("Invalid clientId format", nameof(clientId));
        }

        var metrics = await _metricRepository.GetByClientIdAndYearAsync(
            clientIdLong, 
            year, 
            MetricType.PromotionalSales);

        var totalBenefit = await _metricRepository.GetTotalBenefitAsync(
            clientIdLong, 
            year, 
            MetricType.PromotionalSales);

        var monthlyBreakdown = metrics
            .GroupBy(m => m.Month)
            .Select(g => new MonthlyImpactDto
            {
                Month = g.Key,
                BenefitAmount = g.Sum(m => m.BenefitAmount),
                ItemsCount = g.Sum(m => m.ItemsCount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        return new ImpactMetricResponse
        {
            Year = year,
            TotalBenefit = totalBenefit,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<ImpactMetricResponse> GetOrderImpactAsync(string clientId, int year)
    {
        if (!long.TryParse(clientId, out var clientIdLong))
        {
            throw new ArgumentException("Invalid clientId format", nameof(clientId));
        }

        var metrics = await _metricRepository.GetByClientIdAndYearAsync(
            clientIdLong, 
            year, 
            MetricType.StockoutPrevention);

        var totalBenefit = await _metricRepository.GetTotalBenefitAsync(
            clientIdLong, 
            year, 
            MetricType.StockoutPrevention);

        var monthlyBreakdown = metrics
            .GroupBy(m => m.Month)
            .Select(g => new MonthlyImpactDto
            {
                Month = g.Key,
                BenefitAmount = g.Sum(m => m.BenefitAmount),
                ItemsCount = g.Sum(m => m.ItemsCount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        return new ImpactMetricResponse
        {
            Year = year,
            TotalBenefit = totalBenefit,
            MonthlyBreakdown = monthlyBreakdown
        };
    }
}

