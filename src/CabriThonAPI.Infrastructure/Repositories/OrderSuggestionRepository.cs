using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;
using CabriThonAPI.Infrastructure.Data;

namespace CabriThonAPI.Infrastructure.Repositories;

public class OrderSuggestionRepository : IOrderSuggestionRepository
{
    private readonly ApplicationDbContext _context;

    public OrderSuggestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderSuggestion>> GetByClientIdAsync(
        string clientId, 
        SuggestionStatus? status = null)
    {
        var query = _context.OrderSuggestions
            .Include(o => o.Items)
            .Where(o => o.ClientId == clientId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<OrderSuggestion?> GetByIdAsync(Guid suggestedOrderId)
    {
        return await _context.OrderSuggestions
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.SuggestedOrderId == suggestedOrderId);
    }

    public async Task<OrderSuggestion> AddAsync(OrderSuggestion order)
    {
        _context.OrderSuggestions.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(OrderSuggestion order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.OrderSuggestions.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid suggestedOrderId)
    {
        var order = await GetByIdAsync(suggestedOrderId);
        if (order != null)
        {
            _context.OrderSuggestions.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}

