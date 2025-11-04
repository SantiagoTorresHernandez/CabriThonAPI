using CabriThonAPI.Application.DTOs.Promotions;
using CabriThonAPI.Application.DTOs.Orders;
using CabriThonAPI.Application.Interfaces;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;

namespace CabriThonAPI.Application.Services;

public class SuggestionService : ISuggestionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ISuggestedOrderRepository _orderRepository;

    public SuggestionService(
        IPromotionRepository promotionRepository,
        ISuggestedOrderRepository orderRepository)
    {
        _promotionRepository = promotionRepository;
        _orderRepository = orderRepository;
    }

    public async Task<PromotionSuggestionListResponse> GetPromotionSuggestionsAsync(
        string clientId, 
        string? status, 
        int? limit)
    {
        if (!long.TryParse(clientId, out var clientIdLong))
        {
            throw new ArgumentException("Invalid clientId format", nameof(clientId));
        }

        var promotions = await _promotionRepository.GetByClientIdAsync(clientIdLong, status, limit);

        var dtos = promotions.Select(p => new PromotionSuggestionDto
        {
            PromotionId = p.PromotionId,
            PromotionCode = p.PromotionCode,
            Status = p.Status,
            Name = p.Name,
            Description = p.Description,
            JustificationAI = p.JustificationAI,
            ExpectedIncreasePercent = p.ExpectedIncreasePercent,
            Products = p.PromotionProducts.Select(pp => new PromotionProductDto
            {
                ProductId = pp.ProductId,
                ProductName = pp.Product?.Name ?? string.Empty,
                Quantity = pp.Quantity,
                DiscountApplied = pp.DiscountApplied
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
        if (!long.TryParse(clientId, out var clientIdLong))
        {
            throw new ArgumentException("Invalid clientId format", nameof(clientId));
        }

        int? statusInt = null;
        if (!string.IsNullOrEmpty(status))
        {
            // Map status string to int: Draft=0, Approved=1, Rejected=2, Applied=3
            statusInt = status.ToLower() switch
            {
                "draft" => 0,
                "approved" => 1,
                "rejected" => 2,
                "applied" => 3,
                _ => null
            };
        }

        var orders = await _orderRepository.GetByClientIdAsync(clientIdLong, statusInt);

        var dtos = orders.Select(o => new OrderSuggestionDto
        {
            SuggestedOrderId = o.SuggestedOrderId,
            Status = o.Status switch
            {
                0 => "Draft",
                1 => "Approved",
                2 => "Rejected",
                3 => "Applied",
                _ => "Unknown"
            },
            TotalEstimatedCost = o.SuggestedOrderItems.Sum(i => 
                i.Product != null ? i.Product.Cost * i.Quantity : 0),
            Items = o.SuggestedOrderItems.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                SuggestedQuantity = i.Quantity,
                CurrentStock = 0, // Would need to join with inventory_client
                ReorderPoint = 0, // Would need additional data
                UnitCost = i.Product?.Cost ?? 0,
                Justification = $"Suggested quantity: {i.Quantity}"
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
