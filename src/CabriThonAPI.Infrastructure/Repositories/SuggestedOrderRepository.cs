using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;
using CabriThonAPI.Infrastructure.Data;

namespace CabriThonAPI.Infrastructure.Repositories;

public class SuggestedOrderRepository : ISuggestedOrderRepository
{
    private readonly ApplicationDbContext _context;

    public SuggestedOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SuggestedOrder>> GetByClientIdAsync(
        long clientId, 
        int? status = null)
    {
        var query = _context.SuggestedOrders
            .Include(o => o.SuggestedOrderItems)
                .ThenInclude(i => i.Product)
            .Where(o => o.ClientId == clientId && o.IsActive);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<SuggestedOrder?> GetByIdAsync(long suggestedOrderId)
    {
        return await _context.SuggestedOrders
            .Include(o => o.SuggestedOrderItems)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.SuggestedOrderId == suggestedOrderId);
    }

    public async Task<SuggestedOrder> AddAsync(SuggestedOrder order)
    {
        _context.SuggestedOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(SuggestedOrder order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.SuggestedOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long suggestedOrderId)
    {
        var order = await GetByIdAsync(suggestedOrderId);
        if (order != null)
        {
            order.IsActive = false;
            await UpdateAsync(order);
        }
    }
}

