using FluentAssertions;
using FluentValidation.TestHelper;
using Inventario.Application.DTOs.Stock;
using Inventario.Application.Validators.Stock;
using Inventario.Domain.Enums;

namespace Inventario.UnitTests.Validators;

public class CreateStockMovementValidatorTests
{
    private readonly CreateStockMovementValidator _validator;

    public CreateStockMovementValidatorTests()
    {
        _validator = new CreateStockMovementValidator();
    }

    #region ProductId Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ProductId_WhenInvalid_ShouldHaveValidationError(int productId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.ProductId = productId;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId)
            .WithErrorMessage("Product is required");
    }

    [Fact]
    public void ProductId_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.ProductId = 1;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    #endregion

    #region MovementTypeId Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MovementTypeId_WhenZeroOrNegative_ShouldHaveValidationError(int typeId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MovementTypeId = typeId;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MovementTypeId)
            .WithErrorMessage("Movement type is required");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(99)]
    public void MovementTypeId_WhenOutOfRange_ShouldHaveValidationError(int typeId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MovementTypeId = typeId;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MovementTypeId)
            .WithErrorMessage("Invalid movement type. Valid values: 1 (In), 2 (Out), 3 (Adjustment)");
    }

    [Theory]
    [InlineData(1)] // In
    [InlineData(2)] // Out
    [InlineData(3)] // Adjustment
    public void MovementTypeId_WhenValid_ShouldNotHaveValidationError(int typeId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MovementTypeId = typeId;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MovementTypeId);
    }

    [Fact]
    public void MovementTypeId_ShouldMatchEnum()
    {
        // Assert enum values match expected database values
        ((int)MovementTypeEnum.In).Should().Be(1);
        ((int)MovementTypeEnum.Out).Should().Be(2);
        ((int)MovementTypeEnum.Adjustment).Should().Be(3);
    }

    #endregion

    #region Quantity Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Quantity_WhenZeroOrNegative_ShouldHaveValidationError(int quantity)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Quantity = quantity;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void Quantity_WhenTooLarge_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Quantity = 1000001;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity is too large");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void Quantity_WhenValid_ShouldNotHaveValidationError(int quantity)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Quantity = quantity;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    #endregion

    #region Reason Validation

    [Fact]
    public void Reason_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Reason = new string('A', 501);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 500 characters");
    }

    [Fact]
    public void Reason_WhenNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Reason = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    #endregion

    #region Reference Validation

    [Fact]
    public void Reference_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Reference = new string('A', 101);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reference)
            .WithErrorMessage("Reference cannot exceed 100 characters");
    }

    #endregion

    #region Full Validation

    [Fact]
    public void ValidStockMovement_ShouldPassValidation()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidInMovement_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.In,
            Quantity = 50,
            Reason = "Restock from supplier",
            Reference = "PO-2024-001"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidOutMovement_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.Out,
            Quantity = 10,
            Reason = "Customer sale",
            Reference = "INV-2024-001"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidAdjustment_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.Adjustment,
            Quantity = 25,
            Reason = "Physical inventory count correction"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static CreateStockMovementDto CreateValidDto()
    {
        return new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.In,
            Quantity = 10,
            Reason = "Test reason",
            Reference = "REF-001"
        };
    }

    #endregion
}
