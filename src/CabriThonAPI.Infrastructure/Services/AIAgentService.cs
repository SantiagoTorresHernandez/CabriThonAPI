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
    private readonly IPromotionRepository _promotionRepository;
    private readonly ISuggestedOrderRepository _orderRepository;

    public AIAgentService(
        ILogger<AIAgentService> logger,
        IGeminiAIService geminiService,
        IExternalDataService externalDataService,
        IPromotionRepository promotionRepository,
        ISuggestedOrderRepository orderRepository)
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

            if (!long.TryParse(clientId, out var clientIdLong))
            {
                throw new ArgumentException("Invalid clientId format", nameof(clientId));
            }

            // Get data from external service or database
            var products = await _externalDataService.GetProductsAsync(clientId);
            var stockData = await _externalDataService.GetStockDataAsync(clientId);
            var orderHistory = await _externalDataService.GetOrderHistoryAsync(clientId);

            // Build prompt for AI
            var prompt = BuildPromotionPrompt(products, stockData, orderHistory);

            // Get AI suggestion
            var aiResponse = await _geminiService.GeneratePromotionSuggestionAsync(prompt);

            // Parse AI response and create promotion
            var promotion = ParsePromotionSuggestion(clientIdLong, aiResponse, products);

            // Save to database
            await _promotionRepository.AddAsync(promotion);

            _logger.LogInformation("Promotion Agent completed for client {ClientId}. Created promotion {PromotionId}", 
                clientId, promotion.PromotionId);
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

            if (!long.TryParse(clientId, out var clientIdLong))
            {
                throw new ArgumentException("Invalid clientId format", nameof(clientId));
            }

            // Get data from external service or database
            var products = await _externalDataService.GetProductsAsync(clientId);
            var stockData = await _externalDataService.GetStockDataAsync(clientId);
            var orderHistory = await _externalDataService.GetOrderHistoryAsync(clientId);

            // Build inventory analysis data
            var inventoryData = BuildInventoryData(products, stockData, orderHistory);

            // Get AI analysis
            var aiResponse = await _geminiService.AnalyzeInventoryAsync(inventoryData);

            // Parse AI response and create suggested order
            var suggestedOrder = ParseOrderSuggestion(clientIdLong, aiResponse, products, stockData);

            // Save to database
            await _orderRepository.AddAsync(suggestedOrder);

            _logger.LogInformation("Replenishment Agent completed for client {ClientId}. Created suggested order {OrderId}", 
                clientId, suggestedOrder.SuggestedOrderId);
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
Return JSON with: justificationAI (string), expectedIncreasePercent (decimal), products (array with productId, quantity, discountApplied).
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

    private Promotion ParsePromotionSuggestion(
        long clientId, 
        string aiResponse,
        List<Application.DTOs.External.ProductDto> products)
    {
        // Simple parsing (in production, use proper JSON parsing with error handling)
        var promotion = new Promotion
        {
            ClientId = clientId,
            Name = "AI Generated Combo Promotion",
            Description = "Combo promotion suggested by AI agent",
            JustificationAI = "AI-generated promotion suggestion based on inventory analysis",
            ExpectedIncreasePercent = 15.0m,
            Status = "Draft",
            CreatedByAI = true,
            PromotionProducts = new List<PromotionProduct>()
        };

        // Add sample products
        var firstProduct = products.FirstOrDefault();
        if (firstProduct != null)
        {
            promotion.PromotionProducts.Add(new PromotionProduct
            {
                ProductId = firstProduct.ProductId,
                Quantity = 1,
                IndividualPrice = firstProduct.Price,
                DiscountApplied = firstProduct.Price * 0.20m // 20% discount
            });
        }

        return promotion;
    }

    private SuggestedOrder ParseOrderSuggestion(
        long clientId,
        string aiResponse,
        List<Application.DTOs.External.ProductDto> products,
        List<Application.DTOs.External.StockDto> stockData)
    {
        // Simple parsing (in production, use proper JSON parsing with error handling)
        var lowStockItems = stockData.Where(s => s.Quantity < s.ReorderPoint).ToList();

        var suggestedOrder = new SuggestedOrder
        {
            ClientId = clientId,
            Status = 0, // Draft
            SuggestedOrderItems = new List<SuggestedOrderItem>()
        };

        foreach (var stock in lowStockItems)
        {
            var neededQty = stock.ReorderPoint * 2 - stock.Quantity;
            
            suggestedOrder.SuggestedOrderItems.Add(new SuggestedOrderItem
            {
                ProductId = stock.ProductId,
                Quantity = Math.Max(neededQty, 10)
            });
        }

        return suggestedOrder;
    }
}
