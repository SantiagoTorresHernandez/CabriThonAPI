using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;
using CabriThonAPI.Infrastructure.Data;

namespace CabriThonAPI.Infrastructure.Repositories;

public class PromotionSuggestionRepository : IPromotionSuggestionRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionSuggestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromotionSuggestion>> GetByClientIdAsync(
        string clientId, 
        SuggestionStatus? status = null, 
        int? limit = null)
    {
        var query = _context.PromotionSuggestions
            .Include(p => p.Products)
            .Where(p => p.ClientId == clientId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<PromotionSuggestion?> GetByIdAsync(Guid promotionId)
    {
        return await _context.PromotionSuggestions
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.PromotionId == promotionId);
    }

    public async Task<PromotionSuggestion> AddAsync(PromotionSuggestion promotion)
    {
        _context.PromotionSuggestions.Add(promotion);
        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task UpdateAsync(PromotionSuggestion promotion)
    {
        promotion.UpdatedAt = DateTime.UtcNow;
        _context.PromotionSuggestions.Update(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid promotionId)
    {
        var promotion = await GetByIdAsync(promotionId);
        if (promotion != null)
        {
            _context.PromotionSuggestions.Remove(promotion);
            await _context.SaveChangesAsync();
        }
    }
}

