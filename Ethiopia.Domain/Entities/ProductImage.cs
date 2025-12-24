
namespace Ethiopia.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;
    }
}
