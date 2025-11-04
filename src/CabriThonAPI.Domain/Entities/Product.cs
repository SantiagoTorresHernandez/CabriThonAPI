namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents a product (maps to 'product' table in Supabase)
/// </summary>
public class Product
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public string? Size { get; set; }
    public int? BrandId { get; set; }
    public int? SubbrandId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<PromotionProduct> PromotionProducts { get; set; } = new();
    public List<SuggestedOrderItem> SuggestedOrderItems { get; set; } = new();
    public List<InventoryClient> InventoryClients { get; set; } = new();
}

