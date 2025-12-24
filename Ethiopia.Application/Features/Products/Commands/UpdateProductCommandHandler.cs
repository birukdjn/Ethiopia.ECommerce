using AutoMapper;
using Ethiopia.Application.Interfaces;
using Ethiopia.Domain.Entities;
using MediatR;

namespace Ethiopia.Application.Features.Products.Commands
{
    public class UpdateProductCommandHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            if (request.Price <= 0)
                throw new ApplicationException("Price must be greater than zero.");

            var product = _mapper.Map<Product>(request);
            await _repository.UpdateAsync(product);
        }
    }
}
