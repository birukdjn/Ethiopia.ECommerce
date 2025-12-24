using Ethiopia.Application.Interfaces;
using MediatR;

namespace Ethiopia.Application.Features.Products.Commands
{
    public class DeleteProductCommandHandler(IProductRepository repository) : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductRepository _repository = repository;

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(request.Id);
        }
    }
}
