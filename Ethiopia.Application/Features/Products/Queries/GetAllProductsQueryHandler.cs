using AutoMapper;
using Ethiopia.Application.Features.Products.DTOs;
using Ethiopia.Application.Interfaces;
using MediatR;

namespace Ethiopia.Application.Features.Products.Queries
{
    public class GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper) : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return productDtos;
        }
    }
}
