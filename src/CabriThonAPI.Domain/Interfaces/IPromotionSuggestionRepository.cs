using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Domain.Interfaces;

/// <summary>
/// Repository interface for promotions
/// </summary>
public interface IPromotionRepository
{
    Task<IEnumerable<Promotion>> GetByClientIdAsync(long clientId, string? status = null, int? limit = null);
    Task<Promotion?> GetByIdAsync(long promotionId);
    Task<Promotion> AddAsync(Promotion promotion);
    Task UpdateAsync(Promotion promotion);
    Task DeleteAsync(long promotionId);
}

