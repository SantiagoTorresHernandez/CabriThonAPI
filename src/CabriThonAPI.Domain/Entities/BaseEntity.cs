namespace CabriThonAPI.Domain.Entities;

/// <summary>
/// Base class for all entities in the domain
/// </summary>
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

