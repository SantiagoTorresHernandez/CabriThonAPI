using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;
using CabriThonAPI.Infrastructure.Data;

namespace CabriThonAPI.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Promotion>> GetByClientIdAsync(
        long clientId, 
        string? status = null, 
        int? limit = null)
    {
        var query = _context.Promotions
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Where(p => p.ClientId == clientId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Promotion?> GetByIdAsync(long promotionId)
    {
        return await _context.Promotions
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionMetrics)
            .FirstOrDefaultAsync(p => p.PromotionId == promotionId);
    }

    public async Task<Promotion> AddAsync(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        promotion.UpdatedAt = DateTime.UtcNow;
        _context.Promotions.Update(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long promotionId)
    {
        var promotion = await GetByIdAsync(promotionId);
        if (promotion != null)
        {
            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
        }
    }
}

