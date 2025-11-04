namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents a suggested order (maps to 'suggested_order' table in Supabase)
/// </summary>
public class SuggestedOrder : BaseEntity
{
    public long SuggestedOrderId { get; set; }
    public long ClientId { get; set; }
    public int Status { get; set; } = 0; // 0 = Draft, 1 = Approved, 2 = Rejected, 3 = Applied
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Client? Client { get; set; }
    public List<SuggestedOrderItem> SuggestedOrderItems { get; set; } = new();
}

/// <summary>
/// Represents an item within a suggested order (maps to 'suggested_order_item' table)
/// </summary>
public class SuggestedOrderItem
{
    public long SuggestedOrderItemId { get; set; }
    public long SuggestedOrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    
    // Navigation properties
    public SuggestedOrder? SuggestedOrder { get; set; }
    public Product? Product { get; set; }
}

