using CabriThonAPI.Application.DTOs.External;

namespace CabriThonAPI.Application.Interfaces;

/// <summary>
/// Service interface for external data retrieval from Repository 1
/// </summary>
public interface IExternalDataService
{
    Task<List<ProductDto>> GetProductsAsync(string clientId);
    Task<List<StockDto>> GetStockDataAsync(string clientId);
    Task<List<OrderHistoryDto>> GetOrderHistoryAsync(string clientId);
}

