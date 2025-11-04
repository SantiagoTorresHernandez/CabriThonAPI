namespace CabriThonAPI.Application.DTOs.External;

/// <summary>
/// DTOs for data coming from external systems (Repository 1) or internal DB
/// </summary>
public class ProductDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}

public class StockDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReorderPoint { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class OrderHistoryDto
{
    public long OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
}

