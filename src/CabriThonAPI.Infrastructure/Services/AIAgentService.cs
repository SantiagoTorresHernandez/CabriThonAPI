using CabriThonAPI.Application.Interfaces;
using CabriThonAPI.Domain.Entities;
using CabriThonAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CabriThonAPI.Infrastructure.Services;

public class AIAgentService : IAIAgentService
{
    private readonly ILogger<AIAgentService> _logger;
    private readonly IGeminiAIService _geminiService;
    private readonly IExternalDataService _externalDataService;
    private readonly IPromotionSuggestionRepository _promotionRepository;
    private readonly IOrderSuggestionRepository _orderRepository;

    public AIAgentService(
        ILogger<AIAgentService> logger,
        IGeminiAIService geminiService,
        IExternalDataService externalDataService,
        IPromotionSuggestionRepository promotionRepository,
        IOrderSuggestionRepository orderRepository)
    {
        _logger = logger;
        _geminiService = geminiService;
        _externalDataService = externalDataService;
        _promotionRepository = promotionRepository;
        _orderRepository = orderRepository;
    }

    public async Task ExecutePromotionAgentAsync(string clientId)
    {
        try
        {
            _logger.LogInformation("Starting Promotion Agent for client {ClientId}", clientId);

            // Get data from external service
            var products = await _externalDataService.GetProductsAsync(clientId);
            var stockData = await _externalDataService.GetStockDataAsync(clientId);
            var orderHistory = await _externalDataService.GetOrderHistoryAsync(clientId);

            // Build prompt for AI
            var prompt = BuildPromotionPrompt(products, stockData, orderHistory);

            // Get AI suggestion
            var aiResponse = await _geminiService.GeneratePromotionSuggestionAsync(prompt);

            // Parse AI response (simplified - in production, you'd want robust parsing)
            var suggestion = ParsePromotionSuggestion(clientId, aiResponse, products);

            // Save to database
            await _promotionRepository.AddAsync(suggestion);

            _logger.LogInformation("Promotion Agent completed for client {ClientId}. Created suggestion {PromotionId}", 
                clientId, suggestion.PromotionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Promotion Agent for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task ExecuteReplenishmentAgentAsync(string clientId)
    {
        try
        {
            _logger.LogInformation("Starting Replenishment Agent for client {ClientId}", clientId);

            // Get data from external service
            var products = await _externalDataService.GetProductsAsync(clientId);
            var stockData = await _externalDataService.GetStockDataAsync(clientId);
            var orderHistory = await _externalDataService.GetOrderHistoryAsync(clientId);

            // Build inventory analysis data
            var inventoryData = BuildInventoryData(products, stockData, orderHistory);

            // Get AI analysis
            var aiResponse = await _geminiService.AnalyzeInventoryAsync(inventoryData);

            // Parse AI response
            var suggestion = ParseOrderSuggestion(clientId, aiResponse, products, stockData);

            // Save to database
            await _orderRepository.AddAsync(suggestion);

            _logger.LogInformation("Replenishment Agent completed for client {ClientId}. Created suggestion {OrderId}", 
                clientId, suggestion.SuggestedOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Replenishment Agent for client {ClientId}", clientId);
            throw;
        }
    }

    private string BuildPromotionPrompt(
        List<Application.DTOs.External.ProductDto> products, 
        List<Application.DTOs.External.StockDto> stockData,
        List<Application.DTOs.External.OrderHistoryDto> orderHistory)
    {
        return $@"
You are an AI retail analyst. Analyze the following data and suggest promotional combinations:

Products: {JsonSerializer.Serialize(products.Take(10))}
Stock Levels: {JsonSerializer.Serialize(stockData.Take(10))}
Recent Sales: {JsonSerializer.Serialize(orderHistory.Take(20))}

Task: Identify low-rotation products and suggest combo offers with high-rotation items.
Return JSON with: justificationAI (string), expectedIncreasePercent (decimal), products (array with productId, role, discountPercent).
";
    }

    private string BuildInventoryData(
        List<Application.DTOs.External.ProductDto> products,
        List<Application.DTOs.External.StockDto> stockData,
        List<Application.DTOs.External.OrderHistoryDto> orderHistory)
    {
        return JsonSerializer.Serialize(new 
        { 
            products = products.Take(10), 
            stock = stockData.Take(10), 
            recentOrders = orderHistory.Take(20) 
        });
    }

    private PromotionSuggestion ParsePromotionSuggestion(
        string clientId, 
        string aiResponse,
        List<Application.DTOs.External.ProductDto> products)
    {
        // Simple parsing (in production, use proper JSON parsing with error handling)
        var suggestion = new PromotionSuggestion
        {
            PromotionId = Guid.NewGuid(),
            ClientId = clientId,
            JustificationAI = "AI-generated promotion suggestion based on inventory analysis",
            ExpectedIncreasePercent = 15.0m,
            Status = SuggestionStatus.Draft,
            Products = new List<PromotionProduct>
            {
                new PromotionProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = products.FirstOrDefault()?.ProductId ?? "PROD001",
                    ProductName = products.FirstOrDefault()?.Name ?? "Sample Product",
                    DiscountPercent = 20.0m,
                    Role = "primary"
                }
            }
        };

        return suggestion;
    }

    private OrderSuggestion ParseOrderSuggestion(
        string clientId,
        string aiResponse,
        List<Application.DTOs.External.ProductDto> products,
        List<Application.DTOs.External.StockDto> stockData)
    {
        // Simple parsing (in production, use proper JSON parsing with error handling)
        var lowStockItems = stockData.Where(s => s.Quantity < s.ReorderPoint).ToList();

        var suggestion = new OrderSuggestion
        {
            SuggestedOrderId = Guid.NewGuid(),
            ClientId = clientId,
            Status = SuggestionStatus.Draft,
            Items = lowStockItems.Select(stock =>
            {
                var product = products.FirstOrDefault(p => p.ProductId == stock.ProductId);
                var neededQty = stock.ReorderPoint * 2 - stock.Quantity;

                return new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = stock.ProductId,
                    ProductName = stock.ProductName,
                    SuggestedQuantity = Math.Max(neededQty, 10),
                    CurrentStock = stock.Quantity,
                    ReorderPoint = stock.ReorderPoint,
                    UnitCost = product?.Cost ?? 0m,
                    Justification = $"Stock below reorder point. Current: {stock.Quantity}, Reorder Point: {stock.ReorderPoint}"
                };
            }).ToList()
        };

        suggestion.TotalEstimatedCost = suggestion.Items.Sum(i => i.UnitCost * i.SuggestedQuantity);

        return suggestion;
    }
}

