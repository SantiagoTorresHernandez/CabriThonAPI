namespace CabriThonAPI.Application.DTOs.Promotions;

public class PromotionSuggestionDto
{
    public long PromotionId { get; set; }
    public string? PromotionCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? JustificationAI { get; set; }
    public decimal? ExpectedIncreasePercent { get; set; }
    public List<PromotionProductDto> Products { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty;
}

public class PromotionProductDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? DiscountApplied { get; set; }
}

public class PromotionSuggestionListResponse
{
    public List<PromotionSuggestionDto> Suggestions { get; set; } = new();
    public int Count { get; set; }
}

