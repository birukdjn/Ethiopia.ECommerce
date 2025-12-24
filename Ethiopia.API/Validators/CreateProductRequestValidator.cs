using Ethiopia.API.Controllers;
using FluentValidation;
using Ethiopia.Application.Features.Products.DTOs;

namespace Ethiopia.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Sku)
                .NotEmpty()
                .MaximumLength(50)
                .Matches("^[A-Z0-9-]+$")
                .WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

            RuleFor(x => x.Price)
                .GreaterThan(0);

            RuleFor(x => x.InitialStock)
                .GreaterThanOrEqualTo(0);
        }
    }

}