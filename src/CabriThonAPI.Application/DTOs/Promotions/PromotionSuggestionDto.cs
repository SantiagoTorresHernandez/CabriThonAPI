namespace CabriThonAPI.Application.DTOs.Promotions;

public class PromotionSuggestionDto
{
    public string PromotionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string JustificationAI { get; set; } = string.Empty;
    public decimal ExpectedIncreasePercent { get; set; }
    public List<PromotionProductDto> Products { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty;
}

public class PromotionProductDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class PromotionSuggestionListResponse
{
    public List<PromotionSuggestionDto> Suggestions { get; set; } = new();
    public int Count { get; set; }
}

