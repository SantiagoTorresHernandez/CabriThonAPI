namespace CabriThonAPI.Application.DTOs.Orders;

public class OrderSuggestionDto
{
    public long SuggestedOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalEstimatedCost { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int SuggestedQuantity { get; set; }
    public int CurrentStock { get; set; }
    public int ReorderPoint { get; set; }
    public decimal UnitCost { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class OrderSuggestionListResponse
{
    public List<OrderSuggestionDto> Suggestions { get; set; } = new();
    public int Count { get; set; }
}

