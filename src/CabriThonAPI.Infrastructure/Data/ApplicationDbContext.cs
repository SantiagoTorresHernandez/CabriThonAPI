using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PromotionSuggestion> PromotionSuggestions { get; set; }
    public DbSet<PromotionProduct> PromotionProducts { get; set; }
    public DbSet<OrderSuggestion> OrderSuggestions { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ImpactMetric> ImpactMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PromotionSuggestion configuration
        modelBuilder.Entity<PromotionSuggestion>(entity =>
        {
            entity.HasKey(e => e.PromotionId);
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.JustificationAI).IsRequired();
            entity.Property(e => e.ExpectedIncreasePercent).HasPrecision(5, 2);
            entity.HasIndex(e => new { e.ClientId, e.Status });
        });

        modelBuilder.Entity<PromotionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        // OrderSuggestion configuration
        modelBuilder.Entity<OrderSuggestion>(entity =>
        {
            entity.HasKey(e => e.SuggestedOrderId);
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalEstimatedCost).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.ClientId, e.Status });
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Justification).IsRequired();
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
        });

        // ImpactMetric configuration
        modelBuilder.Entity<ImpactMetric>(entity =>
        {
            entity.HasKey(e => e.MetricId);
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.BenefitAmount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.ClientId, e.Year, e.Type });
        });
    }
}

