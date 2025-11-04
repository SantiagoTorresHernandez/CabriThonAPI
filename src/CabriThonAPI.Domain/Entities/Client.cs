namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents a client/store (maps to 'client' table in Supabase)
/// </summary>
public class Client
{
    public long ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<Promotion> Promotions { get; set; } = new();
    public List<SuggestedOrder> SuggestedOrders { get; set; } = new();
    public List<InventoryClient> InventoryClients { get; set; } = new();
}

