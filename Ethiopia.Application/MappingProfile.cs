using AutoMapper;
using Ethiopia.Domain.Entities;
using Ethiopia.Application.Features.Products.Commands;
using Ethiopia.Application.Features.Products.DTOs;

namespace Ethiopia.Application;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductCommand, Product>();
        CreateMap<DeleteProductCommand, Product>();
    }
}