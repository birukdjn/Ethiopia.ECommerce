using MediatR;

namespace Ethiopia.Application.Features.Products.Commands
{
    public record DeleteProductCommand(Guid Id) : IRequest;
}
