using MediatR;

namespace Ethiopia.Application.Features.Products.Commands
{
    public class CreateProductCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "ETB";
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public int InitialStock { get; set; } = 0;
    }
}
