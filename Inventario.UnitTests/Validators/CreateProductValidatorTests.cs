using FluentAssertions;
using FluentValidation.TestHelper;
using Inventario.Application.DTOs.Products;
using Inventario.Application.Validators.Products;

namespace Inventario.UnitTests.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator;

    public CreateProductValidatorTests()
    {
        _validator = new CreateProductValidator();
    }

    #region SKU Validation

    [Fact]
    public void SKU_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.SKU = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU is required");
    }

    [Fact]
    public void SKU_WhenContainsSpecialCharacters_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.SKU = "PROD@001";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU can only contain alphanumeric characters, hyphens, and underscores");
    }

    [Theory]
    [InlineData("PROD-001")]
    [InlineData("PROD_001")]
    [InlineData("ABC123")]
    public void SKU_WhenValidFormat_ShouldNotHaveValidationError(string sku)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.SKU = sku;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SKU);
    }

    [Fact]
    public void SKU_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.SKU = new string('A', 51);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU cannot exceed 50 characters");
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Name = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Name_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Name = new string('A', 201);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 200 characters");
    }

    #endregion

    #region CategoryId Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CategoryId_WhenInvalid_ShouldHaveValidationError(int categoryId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.CategoryId = categoryId;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category is required");
    }

    #endregion

    #region Price Validation

    [Fact]
    public void UnitPrice_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.UnitPrice = -10m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
            .WithErrorMessage("Unit price cannot be negative");
    }

    [Fact]
    public void Cost_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Cost = -5m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cost)
            .WithErrorMessage("Cost cannot be negative");
    }

    #endregion

    #region Quantity Validation

    [Fact]
    public void Quantity_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Quantity = -1;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity cannot be negative");
    }

    [Fact]
    public void Quantity_WhenZero_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Quantity = 0;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    #endregion

    #region Stock Limits Validation

    [Fact]
    public void MinStock_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MinStock = -1;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinStock)
            .WithErrorMessage("Minimum stock cannot be negative");
    }

    [Fact]
    public void MaxStock_WhenLessThanMinStock_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MinStock = 10;
        dto.MaxStock = 5;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxStock)
            .WithErrorMessage("Maximum stock must be greater than minimum stock");
    }

    [Fact]
    public void MaxStock_WhenGreaterThanMinStock_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.MinStock = 5;
        dto.MaxStock = 100;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxStock);
    }

    #endregion

    #region Full Validation

    [Fact]
    public void ValidCreateProductRequest_ShouldPassValidation()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static CreateProductDto CreateValidDto()
    {
        return new CreateProductDto
        {
            SKU = "PROD-001",
            Name = "Test Product",
            Description = "Test Description",
            CategoryId = 1,
            UnitPrice = 99.99m,
            Cost = 50m,
            Quantity = 10,
            MinStock = 5,
            MaxStock = 100
        };
    }

    #endregion
}
