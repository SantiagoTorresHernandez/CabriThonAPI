using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for order suggestions
/// </summary>
public interface IOrderSuggestionRepository
{
    Task<IEnumerable<OrderSuggestion>> GetByClientIdAsync(string clientId, SuggestionStatus? status = null);
    Task<OrderSuggestion?> GetByIdAsync(Guid suggestedOrderId);
    Task<OrderSuggestion> AddAsync(OrderSuggestion order);
    Task UpdateAsync(OrderSuggestion order);
    Task DeleteAsync(Guid suggestedOrderId);
}

