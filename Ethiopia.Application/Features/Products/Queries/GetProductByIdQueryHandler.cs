using AutoMapper;
using Ethiopia.Application.Features.Products.DTOs;
using Ethiopia.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ethiopia.Application.Features.Products.Queries
{
    public class GetProductByIdQueryHandler(IProductRepository repo, IMapper mapper)
          : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _repo = repo;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync(request.Id);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }
    }

}
