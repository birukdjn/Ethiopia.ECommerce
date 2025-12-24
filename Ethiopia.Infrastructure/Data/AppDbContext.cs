using Ethiopia.Domain.Entities;
using Ethiopia.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ethiopia.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Inventory> Inventories => Set<Inventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(p => p.Sku)
                .IsUnique();

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(p => p.DiscountPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.AverageRating)
                .HasColumnType("decimal(3,2)");

            entity.HasQueryFilter(p => !p.IsDeleted);

            // Indexes
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.Brand);
            entity.HasIndex(p => p.IsActive);
            entity.HasIndex(p => p.CreatedAt);
        });

        // ProductImage configuration
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(pi => pi.Id);

            entity.Property(pi => pi.ImageUrl)
                .HasMaxLength(500);

            entity.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductReview configuration
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(pr => pr.Id);

            entity.Property(pr => pr.Comment)
                .HasMaxLength(1000);

            entity.Property(pr => pr.Rating)
                .IsRequired();

            entity.HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set audit fields
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditableEntity &&
                       (e.State == EntityState.Modified || e.State == EntityState.Added));

        foreach (var entityEntry in entries)
        {
            var entity = (IAuditableEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}