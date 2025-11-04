using CabriThonAPI.Application.DTOs.Promotions;
using CabriThonAPI.Application.DTOs.Orders;
using CabriThonAPI.Application.Interfaces;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;

namespace CabriThonAPI.Application.Services;

public class SuggestionService : ISuggestionService
{
    private readonly IPromotionSuggestionRepository _promotionRepository;
    private readonly IOrderSuggestionRepository _orderRepository;

    public SuggestionService(
        IPromotionSuggestionRepository promotionRepository,
        IOrderSuggestionRepository orderRepository)
    {
        _promotionRepository = promotionRepository;
        _orderRepository = orderRepository;
    }

    public async Task<PromotionSuggestionListResponse> GetPromotionSuggestionsAsync(
        string clientId, 
        string? status, 
        int? limit)
    {
        SuggestionStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status))
        {
            Enum.TryParse<SuggestionStatus>(status, true, out var parsedStatus);
            statusEnum = parsedStatus;
        }

        var promotions = await _promotionRepository.GetByClientIdAsync(clientId, statusEnum, limit);

        var dtos = promotions.Select(p => new PromotionSuggestionDto
        {
            PromotionId = p.PromotionId.ToString(),
            Status = p.Status.ToString(),
            JustificationAI = p.JustificationAI,
            ExpectedIncreasePercent = p.ExpectedIncreasePercent,
            Products = p.Products.Select(pp => new PromotionProductDto
            {
                ProductId = pp.ProductId,
                ProductName = pp.ProductName,
                DiscountPercent = pp.DiscountPercent,
                Role = pp.Role
            }).ToList(),
            CreatedAt = p.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        }).ToList();

        return new PromotionSuggestionListResponse
        {
            Suggestions = dtos,
            Count = dtos.Count
        };
    }

    public async Task<OrderSuggestionListResponse> GetOrderSuggestionsAsync(
        string clientId, 
        string? status)
    {
        SuggestionStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status))
        {
            Enum.TryParse<SuggestionStatus>(status, true, out var parsedStatus);
            statusEnum = parsedStatus;
        }

        var orders = await _orderRepository.GetByClientIdAsync(clientId, statusEnum);

        var dtos = orders.Select(o => new OrderSuggestionDto
        {
            SuggestedOrderId = o.SuggestedOrderId.ToString(),
            Status = o.Status.ToString(),
            TotalEstimatedCost = o.TotalEstimatedCost,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                SuggestedQuantity = i.SuggestedQuantity,
                CurrentStock = i.CurrentStock,
                ReorderPoint = i.ReorderPoint,
                UnitCost = i.UnitCost,
                Justification = i.Justification
            }).ToList(),
            CreatedAt = o.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        }).ToList();

        return new OrderSuggestionListResponse
        {
            Suggestions = dtos,
            Count = dtos.Count
        };
    }
}

