using Ethiopia.Application.Features.Products.DTOs;
using MediatR;

namespace Ethiopia.Application.Features.Products.Queries
{
    public class GetAllProductsQuery: IRequest<List<ProductDto>>
    {
    }
}
