using FluentAssertions;
using FluentValidation.TestHelper;
using Inventario.Application.DTOs.Auth;
using Inventario.Application.Validators.Auth;

namespace Inventario.UnitTests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    #region Password Complexity Tests

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Password_WhenTooShort_ShouldHaveValidationError(string password)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Password = password;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_WhenMissingUppercase_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Password = "password123!";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Fact]
    public void Password_WhenMissingLowercase_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Password = "PASSWORD123!";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Fact]
    public void Password_WhenMissingNumber_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Password = "Password!!!";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number");
    }

    [Fact]
    public void Password_WhenMissingSpecialChar_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Password = "Password123";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one special character");
    }

    [Fact]
    public void Password_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Password = "ValidPass123!";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public void FirstName_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.FirstName = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void FirstName_WhenContainsNumbers_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.FirstName = "John123";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name can only contain letters");
    }

    [Fact]
    public void FirstName_WhenContainsAccents_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.FirstName = "José María";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void LastName_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.LastName = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Email = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Email_WhenInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Email = "not-an-email";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    #endregion

    #region Phone Validation Tests

    [Fact]
    public void Phone_WhenInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Phone = "invalid@phone";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Invalid phone format");
    }

    [Theory]
    [InlineData("+51 999 888 777")]
    [InlineData("999-888-777")]
    [InlineData("(01) 234-5678")]
    public void Phone_WhenValidFormat_ShouldNotHaveValidationError(string phone)
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Phone = phone;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Phone_WhenNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto.Phone = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    #endregion

    #region Full Validation

    [Fact]
    public void ValidRegisterRequest_ShouldPassValidation()
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

    private static RegisterRequestDto CreateValidDto()
    {
        return new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = null,
            DocumentNumber = null
        };
    }

    #endregion
}
