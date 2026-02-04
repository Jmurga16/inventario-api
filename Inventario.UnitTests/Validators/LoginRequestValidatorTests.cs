using FluentAssertions;
using FluentValidation.TestHelper;
using Inventario.Application.DTOs.Auth;
using Inventario.Application.Validators.Auth;

namespace Inventario.UnitTests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    #region Email Validation

    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "", Password = "password" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "invalid-email", Password = "password" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Email_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com";
        var dto = new LoginRequestDto { Email = longEmail, Password = "password" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email cannot exceed 256 characters");
    }

    [Fact]
    public void Email_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "valid@email.com", Password = "password" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation

    [Fact]
    public void Password_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Password_WhenTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "12345" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters");
    }

    [Fact]
    public void Password_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = new string('a', 101)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password cannot exceed 100 characters");
    }

    [Fact]
    public void Password_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "validPassword" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Full Validation

    [Fact]
    public void ValidLoginRequest_ShouldPassValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
