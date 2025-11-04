using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for suggested orders
/// </summary>
public interface ISuggestedOrderRepository
{
    Task<IEnumerable<SuggestedOrder>> GetByClientIdAsync(long clientId, int? status = null);
    Task<SuggestedOrder?> GetByIdAsync(long suggestedOrderId);
    Task<SuggestedOrder> AddAsync(SuggestedOrder order);
    Task UpdateAsync(SuggestedOrder order);
    Task DeleteAsync(long suggestedOrderId);
}

