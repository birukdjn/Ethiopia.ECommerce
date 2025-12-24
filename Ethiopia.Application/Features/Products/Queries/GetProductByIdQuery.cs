using Ethiopia.Application.Features.Products.DTOs;
using MediatR;

namespace Ethiopia.Application.Features.Products.Queries
{
    public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

}
