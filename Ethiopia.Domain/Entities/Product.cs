using Ethiopia.Domain.Entities.Interfaces;
using Ethiopia.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ethiopia.Domain.Entities;

public class Product : IAuditableEntity, ISoftDelete
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get;  set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { 
        get =>field; 
        set => field = value < 0 ? throw new ArgumentException():value; }
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [MaxLength(50)]
    public string Sku { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    public decimal? DiscountPrice { get; set; }

    [Range(0, 5)]
    public decimal AverageRating { get; set; } = 0;

    public int ReviewCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<ProductImage> Images { get; set; } = [];
    public virtual ICollection<ProductReview> Reviews { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Additional properties for currency
    [MaxLength(3)]
    [Required]
    public string Currency { get; private set; } = "ETB";

    // Constructor for EF Core
    public Product() { }

    public Product(
        string name,
        string sku,
        string description,
        Money price,
        string? category,
        string? brand,
        int initialStock = 0)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        Description = description;
        Price = price.Amount;
        Currency = price.Currency;
        Category = category;
        Brand = brand;
        StockQuantity = initialStock;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsInStock() => StockQuantity > 0;
    public bool IsOutOfStock() => StockQuantity <= 0;
    public bool HasSufficientStock(int quantity) => StockQuantity >= quantity;

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (!HasSufficientStock(quantity))
            throw new InsufficientStockException(quantity, StockQuantity);

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));

        Price = newPrice.Amount;
        Currency = newPrice.Currency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAverageRating(decimal newRating)
    {
        if (newRating < 0 || newRating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(newRating));

        AverageRating = ((AverageRating * ReviewCount) + newRating) / (ReviewCount + 1);
        ReviewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));

        DiscountPrice = Price * (1 - discountPercentage / 100);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDiscount()
    {
        DiscountPrice = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetCurrentPrice() => DiscountPrice ?? Price;

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class InsufficientStockException(int requested, int available) : Exception($"Insufficient stock. Requested: {requested}, Available: {available}")
{
    public int RequestedQuantity { get; } = requested;
    public int AvailableQuantity { get; } = available;
}