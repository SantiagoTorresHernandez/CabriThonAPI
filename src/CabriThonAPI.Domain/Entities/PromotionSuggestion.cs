namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents a promotion (maps to 'promotion' table in Supabase)
/// </summary>
public class Promotion : BaseEntity
{
    public long PromotionId { get; set; }
    public string? PromotionCode { get; set; }
    public long ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? JustificationAI { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? ExpectedIncreasePercent { get; set; }
    public decimal? ProfitMarginPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // 'Active', 'Inactive', 'Draft'
    public bool CreatedByAI { get; set; } = false;
    
    // Navigation properties
    public Client? Client { get; set; }
    public List<PromotionProduct> PromotionProducts { get; set; } = new();
    public List<PromotionMetric> PromotionMetrics { get; set; } = new();
}

/// <summary>
/// Represents a product within a promotion (maps to 'promotion_product' table)
/// </summary>
public class PromotionProduct
{
    public long PromotionProductId { get; set; }
    public long PromotionId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? IndividualPrice { get; set; }
    public decimal? DiscountApplied { get; set; }
    
    // Navigation properties
    public Promotion? Promotion { get; set; }
    public Product? Product { get; set; }
}

/// <summary>
/// Represents promotion metrics (maps to 'promotion_metrics' table)
/// </summary>
public class PromotionMetric
{
    public long PromotionMetricId { get; set; }
    public long PromotionId { get; set; }
    public decimal? ActualSalesIncrease { get; set; }
    public decimal? ActualProfitMargin { get; set; }
    public decimal? TotalSalesDuringPromo { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Promotion? Promotion { get; set; }
}

