using Ethiopia.Domain.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Ethiopia.Domain.Entities;

public class Category : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Category> SubCategories { get; set; } = [];
    public virtual ICollection<Product> Products { get; set; } = [];

    // IAuditableEntity implementation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}