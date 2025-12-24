namespace Ethiopia.Application.Features.Products.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }

    }
}
