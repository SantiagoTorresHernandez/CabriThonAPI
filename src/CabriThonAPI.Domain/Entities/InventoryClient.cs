namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Represents inventory at client location (maps to 'inventory_client' table in Supabase)
/// </summary>
public class InventoryClient
{
    public long InventoryClientId { get; set; }
    public long ProductId { get; set; }
    public long ClientId { get; set; }
    public int Stock { get; set; } = 0;
    public string? WarehouseLocation { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Product? Product { get; set; }
    public Client? Client { get; set; }
}

