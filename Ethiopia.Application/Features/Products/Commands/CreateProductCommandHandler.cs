using AutoMapper;
using Ethiopia.Application.Interfaces;
using Ethiopia.Domain.Entities;
using Ethiopia.Domain.ValueObjects;
using MediatR;

namespace Ethiopia.Application.Features.Products.Commands
{
  

    public class CreateProductCommandHandler(
        IProductRepository productRepository,
        IMapper mapper) : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Create Money value object
            var price = new Money(request.Price, request.Currency);

            // Check if SKU already exists
            if (await _productRepository.SkuExistsAsync(request.Sku, cancellationToken))
            {
                throw new InvalidOperationException($"Product with SKU '{request.Sku}' already exists.");
            }

            // Create product using constructor
            var product = new Product(
                name: request.Name,
                sku: request.Sku,
                description: request.Description,
                price: price,
                category: request.Category,
                brand: request.Brand,
                initialStock: request.InitialStock
            )
            {
                // Set additional properties using domain methods if needed
                CreatedBy = "System" // Or get from current user context
            };

            var newProductId = await _productRepository.AddAsync(product, cancellationToken);
            return newProductId;
        }
    }

    // AutoMapper Profile
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<CreateProductCommand, Product>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()) // Handled in constructor
                .ForMember(dest => dest.Currency, opt => opt.Ignore()) // Handled in constructor
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StockQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }
    }
}