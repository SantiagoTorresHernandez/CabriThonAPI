using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;
using CabriThonAPI.Infrastructure.Data;

namespace CabriThonAPI.Infrastructure.Repositories;

public class ImpactMetricRepository : IImpactMetricRepository
{
    private readonly ApplicationDbContext _context;

    public ImpactMetricRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ImpactMetric>> GetByClientIdAndYearAsync(
        long clientId, 
        int year, 
        MetricType? type = null)
    {
        // For promotional sales, calculate from promotion_metrics
        if (type == MetricType.PromotionalSales)
        {
            var promotionMetrics = await _context.PromotionMetrics
                .Include(pm => pm.Promotion)
                .Where(pm => pm.Promotion!.ClientId == clientId 
                    && pm.AnalysisDate.Year == year
                    && pm.Promotion.Status == "Active")
                .GroupBy(pm => pm.AnalysisDate.Month)
                .Select(g => new ImpactMetric
                {
                    ClientId = clientId,
                    Year = year,
                    Month = g.Key,
                    Type = MetricType.PromotionalSales,
                    BenefitAmount = g.Sum(pm => pm.ActualSalesIncrease ?? 0),
                    ItemsCount = g.Count(),
                    Description = $"Promotional sales for month {g.Key}"
                })
                .ToListAsync();

            return promotionMetrics;
        }

        // For stockout prevention, calculate from suggested_orders that were applied
        if (type == MetricType.StockoutPrevention)
        {
            var orderMetrics = await _context.SuggestedOrders
                .Include(so => so.SuggestedOrderItems)
                    .ThenInclude(soi => soi.Product)
                .Where(so => so.ClientId == clientId 
                    && so.CreatedAt.Year == year
                    && so.Status == 3) // Applied
                .GroupBy(so => so.CreatedAt.Month)
                .Select(g => new ImpactMetric
                {
                    ClientId = clientId,
                    Year = year,
                    Month = g.Key,
                    Type = MetricType.StockoutPrevention,
                    // Estimate benefit as sum of products cost * quantity (avoided stockouts)
                    BenefitAmount = g.SelectMany(so => so.SuggestedOrderItems)
                        .Sum(soi => soi.Product!.Cost * soi.Quantity),
                    ItemsCount = g.Count(),
                    Description = $"Stockout prevention for month {g.Key}"
                })
                .ToListAsync();

            return orderMetrics;
        }

        // Return empty list if type not specified
        return new List<ImpactMetric>();
    }

    public async Task<decimal> GetTotalBenefitAsync(long clientId, int year, MetricType type)
    {
        var metrics = await GetByClientIdAndYearAsync(clientId, year, type);
        return metrics.Sum(m => m.BenefitAmount);
    }
}
