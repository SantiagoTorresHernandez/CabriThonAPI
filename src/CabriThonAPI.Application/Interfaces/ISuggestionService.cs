using CabriThonAPI.Application.DTOs.Promotions;
using CabriThonAPI.Application.DTOs.Orders;

namespace CabriThonAPI.Application.Interfaces;

/// <summary>
/// Service interface for managing suggestions
/// </summary>
public interface ISuggestionService
{
    Task<PromotionSuggestionListResponse> GetPromotionSuggestionsAsync(string clientId, string? status, int? limit);
    Task<OrderSuggestionListResponse> GetOrderSuggestionsAsync(string clientId, string? status);
}

