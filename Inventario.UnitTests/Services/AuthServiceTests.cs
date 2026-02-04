using FluentAssertions;
using Inventario.Application.DTOs.Auth;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;
using Inventario.Domain.Interfaces.Services;
using Moq;

namespace Inventario.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };

        var user = new User
        {
            Id = 1,
            PersonId = 1,
            Email = "admin@test.com",
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        var person = new Person
        {
            Id = 1,
            FirstName = "Admin",
            LastName = "User"
        };

        var roles = new List<string> { "Admin" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(request.Password, user.PasswordHash))
            .Returns(true);
        _userRepositoryMock.Setup(r => r.GetUserRolesAsync(user.Id))
            .ReturnsAsync(roles);
        _userRepositoryMock.Setup(r => r.GetPersonByIdAsync(user.PersonId))
            .ReturnsAsync(person);
        _jwtServiceMock.Setup(j => j.GenerateToken(user, roles))
            .Returns("jwt_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.FullName.Should().Be("Admin User");
        result.Token.Should().Be("jwt_token");
        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "nonexistent@test.com",
            Password = "password"
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "admin@test.com",
            Password = "wrongPassword"
        };

        var user = new User
        {
            Id = 1,
            Email = "admin@test.com",
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "inactive@test.com",
            Password = "password"
        };

        var user = new User
        {
            Id = 1,
            Email = "inactive@test.com",
            PasswordHash = "hashedPassword",
            IsActive = false
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(request));
    }

    #endregion

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsLoginResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "newuser@test.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        var role = new Role { Id = 2, Name = "Empleado" };
        var roles = new List<string> { "Empleado" };

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.AddPersonAsync(It.IsAny<Person>()))
            .ReturnsAsync((Person p) => { p.Id = 1; return p; });
        _userRepositoryMock.Setup(r => r.GetRoleByNameAsync("Empleado"))
            .ReturnsAsync(role);
        _userRepositoryMock.Setup(r => r.GetUserRolesAsync(It.IsAny<int>()))
            .ReturnsAsync(roles);
        _passwordHasherMock.Setup(p => p.Hash(request.Password))
            .Returns("hashedPassword");
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>(), roles))
            .Returns("jwt_token");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email.ToLower());
        result.FullName.Should().Be("New User");
        result.Token.Should().Be("jwt_token");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "existing@test.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _authService.RegisterAsync(request));
    }

    #endregion
}
