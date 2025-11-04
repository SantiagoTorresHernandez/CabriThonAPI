namespace CabriThonAPI.Application.DTOs.External;

/// <summary>
/// DTOs for data coming from external systems (Repository 1)
/// </summary>
public class ProductDto
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}

public class StockDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReorderPoint { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class OrderHistoryDto
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
}

