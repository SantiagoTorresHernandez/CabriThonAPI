using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for promotion suggestions
/// </summary>
public interface IPromotionSuggestionRepository
{
    Task<IEnumerable<PromotionSuggestion>> GetByClientIdAsync(string clientId, SuggestionStatus? status = null, int? limit = null);
    Task<PromotionSuggestion?> GetByIdAsync(Guid promotionId);
    Task<PromotionSuggestion> AddAsync(PromotionSuggestion promotion);
    Task UpdateAsync(PromotionSuggestion promotion);
    Task DeleteAsync(Guid promotionId);
}

