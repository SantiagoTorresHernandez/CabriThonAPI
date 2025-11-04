using Microsoft.EntityFrameworkCore;
using CabriThonAPI.Domain.Entities;

namespace CabriThonAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<PromotionProduct> PromotionProducts { get; set; }
    public DbSet<PromotionMetric> PromotionMetrics { get; set; }
    public DbSet<SuggestedOrder> SuggestedOrders { get; set; }
    public DbSet<SuggestedOrderItem> SuggestedOrderItems { get; set; }
    public DbSet<InventoryClient> InventoryClients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema for Supabase (public)
        modelBuilder.HasDefaultSchema("public");

        // Client configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("client");
            entity.HasKey(e => e.ClientId);
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Cost).HasColumnName("cost").HasPrecision(18, 2);
            entity.Property(e => e.SuggestedPrice).HasColumnName("suggested_price").HasPrecision(18, 2);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.SubcategoryId).HasColumnName("subcategory_id");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.SubbrandId).HasColumnName("subbrand_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Promotion configuration
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("promotion");
            entity.HasKey(e => e.PromotionId);
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.PromotionCode).HasColumnName("promotion_code");
            entity.Property(e => e.ClientId).HasColumnName("client_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.JustificationAI).HasColumnName("justification_ai");
            entity.Property(e => e.OriginalPrice).HasColumnName("original_price").HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2);
            entity.Property(e => e.FinalPrice).HasColumnName("final_price").HasPrecision(18, 2);
            entity.Property(e => e.ExpectedIncreasePercent).HasColumnName("expected_increase_percent").HasPrecision(5, 2);
            entity.Property(e => e.ProfitMarginPercent).HasColumnName("profit_margin_percent").HasPrecision(5, 2);
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("Draft");
            entity.Property(e => e.CreatedByAI).HasColumnName("created_by_ai").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Promotions)
                .HasForeignKey(e => e.ClientId);

            entity.HasIndex(e => new { e.ClientId, e.Status });
        });

        // PromotionProduct configuration
        modelBuilder.Entity<PromotionProduct>(entity =>
        {
            entity.ToTable("promotion_product");
            entity.HasKey(e => e.PromotionProductId);
            entity.Property(e => e.PromotionProductId).HasColumnName("promotion_product_id");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id").IsRequired();
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(1);
            entity.Property(e => e.IndividualPrice).HasColumnName("individual_price").HasPrecision(18, 2);
            entity.Property(e => e.DiscountApplied).HasColumnName("discount_applied").HasPrecision(18, 2);

            entity.HasOne(e => e.Promotion)
                .WithMany(p => p.PromotionProducts)
                .HasForeignKey(e => e.PromotionId);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.PromotionProducts)
                .HasForeignKey(e => e.ProductId);
        });

        // PromotionMetric configuration
        modelBuilder.Entity<PromotionMetric>(entity =>
        {
            entity.ToTable("promotion_metrics");
            entity.HasKey(e => e.PromotionMetricId);
            entity.Property(e => e.PromotionMetricId).HasColumnName("promotion_metric_id");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id").IsRequired();
            entity.Property(e => e.ActualSalesIncrease).HasColumnName("actual_sales_increase").HasPrecision(18, 2);
            entity.Property(e => e.ActualProfitMargin).HasColumnName("actual_profit_margin").HasPrecision(18, 2);
            entity.Property(e => e.TotalSalesDuringPromo).HasColumnName("total_sales_during_promo").HasPrecision(18, 2);
            entity.Property(e => e.AnalysisDate).HasColumnName("analysis_date");

            entity.HasOne(e => e.Promotion)
                .WithMany(p => p.PromotionMetrics)
                .HasForeignKey(e => e.PromotionId);
        });

        // SuggestedOrder configuration
        modelBuilder.Entity<SuggestedOrder>(entity =>
        {
            entity.ToTable("suggested_order");
            entity.HasKey(e => e.SuggestedOrderId);
            entity.Property(e => e.SuggestedOrderId).HasColumnName("suggested_order_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Client)
                .WithMany(c => c.SuggestedOrders)
                .HasForeignKey(e => e.ClientId);

            entity.HasIndex(e => new { e.ClientId, e.Status });
        });

        // SuggestedOrderItem configuration
        modelBuilder.Entity<SuggestedOrderItem>(entity =>
        {
            entity.ToTable("suggested_order_item");
            entity.HasKey(e => e.SuggestedOrderItemId);
            entity.Property(e => e.SuggestedOrderItemId).HasColumnName("suggested_order_item_id");
            entity.Property(e => e.SuggestedOrderId).HasColumnName("suggested_order_id").IsRequired();
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();

            entity.HasOne(e => e.SuggestedOrder)
                .WithMany(o => o.SuggestedOrderItems)
                .HasForeignKey(e => e.SuggestedOrderId);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.SuggestedOrderItems)
                .HasForeignKey(e => e.ProductId);
        });

        // InventoryClient configuration
        modelBuilder.Entity<InventoryClient>(entity =>
        {
            entity.ToTable("inventory_client");
            entity.HasKey(e => e.InventoryClientId);
            entity.Property(e => e.InventoryClientId).HasColumnName("inventory_client_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.ClientId).HasColumnName("client_id").IsRequired();
            entity.Property(e => e.Stock).HasColumnName("stock").HasDefaultValue(0);
            entity.Property(e => e.WarehouseLocation).HasColumnName("warehouse_location");
            entity.Property(e => e.LastUpdated).HasColumnName("last_updated");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.InventoryClients)
                .HasForeignKey(e => e.ProductId);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.InventoryClients)
                .HasForeignKey(e => e.ClientId);
        });
    }
}
