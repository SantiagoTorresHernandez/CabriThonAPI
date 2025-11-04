using CabriThonAPI.Application.DTOs.External;
using CabriThonAPI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CabriThonAPI.Infrastructure.Services;

public class ExternalDataService : IExternalDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalDataService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public ExternalDataService(
        HttpClient httpClient,
        ILogger<ExternalDataService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _baseUrl = _configuration["ExternalAPI:BaseUrl"] ?? "https://api.repository1.com";
    }

    public async Task<List<ProductDto>> GetProductsAsync(string clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products?clientId={clientId}");
            response.EnsureSuccessStatusCode();
            
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            return products ?? new List<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from external API for client {ClientId}", clientId);
            
            // Return mock data for development
            return new List<ProductDto>
            {
                new ProductDto 
                { 
                    ProductId = 1, 
                    Name = "Sample Product 1", 
                    Category = "Electronics", 
                    Price = 199.99m, 
                    Cost = 120.00m 
                },
                new ProductDto 
                { 
                    ProductId = 2, 
                    Name = "Sample Product 2", 
                    Category = "Accessories", 
                    Price = 49.99m, 
                    Cost = 25.00m 
                }
            };
        }
    }

    public async Task<List<StockDto>> GetStockDataAsync(string clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/stock?clientId={clientId}");
            response.EnsureSuccessStatusCode();
            
            var stock = await response.Content.ReadFromJsonAsync<List<StockDto>>();
            return stock ?? new List<StockDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching stock data from external API for client {ClientId}", clientId);
            
            // Return mock data for development
            return new List<StockDto>
            {
                new StockDto 
                { 
                    ProductId = 1, 
                    ProductName = "Sample Product 1", 
                    Quantity = 25, 
                    ReorderPoint = 50,
                    Location = "Warehouse A"
                },
                new StockDto 
                { 
                    ProductId = 2, 
                    ProductName = "Sample Product 2", 
                    Quantity = 100, 
                    ReorderPoint = 30,
                    Location = "Warehouse A"
                }
            };
        }
    }

    public async Task<List<OrderHistoryDto>> GetOrderHistoryAsync(string clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/orders/history?clientId={clientId}");
            response.EnsureSuccessStatusCode();
            
            var orders = await response.Content.ReadFromJsonAsync<List<OrderHistoryDto>>();
            return orders ?? new List<OrderHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order history from external API for client {ClientId}", clientId);
            
            // Return mock data for development
            return new List<OrderHistoryDto>
            {
                new OrderHistoryDto 
                { 
                    OrderId = 1, 
                    OrderDate = DateTime.UtcNow.AddDays(-7),
                    ProductId = 1, 
                    Quantity = 5, 
                    TotalAmount = 999.95m 
                },
                new OrderHistoryDto 
                { 
                    OrderId = 2, 
                    OrderDate = DateTime.UtcNow.AddDays(-14),
                    ProductId = 2, 
                    Quantity = 10, 
                    TotalAmount = 499.90m 
                }
            };
        }
    }
}

