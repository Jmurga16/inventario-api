using FluentValidation;
using Inventario.Application.DTOs.Products;

namespace Inventario.Application.Validators.Products;

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Unit price is too large");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Cost is too large")
            .When(x => x.Cost.HasValue);

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative")
            .LessThanOrEqualTo(1000000).WithMessage("Minimum stock is too large");

        RuleFor(x => x.MaxStock)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum stock cannot be negative")
            .GreaterThan(x => x.MinStock).WithMessage("Maximum stock must be greater than minimum stock")
            .When(x => x.MaxStock.HasValue);
    }
}
