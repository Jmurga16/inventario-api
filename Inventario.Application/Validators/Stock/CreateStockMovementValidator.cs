using FluentValidation;
using Inventario.Application.DTOs.Stock;

namespace Inventario.Application.Validators.Stock;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementDto>
{
    public CreateStockMovementValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product is required");

        RuleFor(x => x.MovementTypeId)
            .GreaterThan(0).WithMessage("Movement type is required")
            .LessThanOrEqualTo(3).WithMessage("Invalid movement type");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Quantity is too large");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));
    }
}
