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
        string clientId, 
        int year, 
        MetricType? type = null)
    {
        var query = _context.ImpactMetrics
            .Where(m => m.ClientId == clientId && m.Year == year);

        if (type.HasValue)
        {
            query = query.Where(m => m.Type == type.Value);
        }

        return await query
            .OrderBy(m => m.Month)
            .ToListAsync();
    }

    public async Task<ImpactMetric?> GetByIdAsync(Guid metricId)
    {
        return await _context.ImpactMetrics
            .FirstOrDefaultAsync(m => m.MetricId == metricId);
    }

    public async Task<ImpactMetric> AddAsync(ImpactMetric metric)
    {
        _context.ImpactMetrics.Add(metric);
        await _context.SaveChangesAsync();
        return metric;
    }

    public async Task UpdateAsync(ImpactMetric metric)
    {
        metric.UpdatedAt = DateTime.UtcNow;
        _context.ImpactMetrics.Update(metric);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalBenefitAsync(string clientId, int year, MetricType type)
    {
        return await _context.ImpactMetrics
            .Where(m => m.ClientId == clientId && m.Year == year && m.Type == type)
            .SumAsync(m => m.BenefitAmount);
    }
}

