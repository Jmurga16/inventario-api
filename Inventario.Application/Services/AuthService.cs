using Inventario.Application.DTOs.Auth;
using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;
using Inventario.Domain.Interfaces.Services;

namespace Inventario.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        var PasswordHash = _passwordHasher.Hash(request.Password);

        if (user == null)
            throw new UnauthorizedException("Invalid credentials");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedException("User is inactive");

        var roles = await GetUserRolesAsync(user.Id);
        var person = await GetPersonAsync(user.PersonId);
        var token = _jwtService.GenerateToken(user, roles);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{person?.FirstName} {person?.LastName}".Trim(),
            Token = token,
            Roles = roles
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            throw new ValidationException("Email already exists");

        var person = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DocumentTypeId = request.DocumentTypeId,
            DocumentNumber = request.DocumentNumber,
            Phone = request.Phone
        };

        await _unitOfWork.SaveChangesAsync();

        var user = new User
        {
            PersonId = person.Id,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var defaultRole = await GetDefaultRoleAsync();
        if (defaultRole != null)
        {
            await AssignRoleAsync(user.Id, defaultRole.Id);
        }

        var roles = await GetUserRolesAsync(user.Id);
        var token = _jwtService.GenerateToken(user, roles);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{person.FirstName} {person.LastName}".Trim(),
            Token = token,
            Roles = roles
        };
    }

    private async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        var userRoles = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);
        // Manual query since we don't have navigation properties
        return [];
    }

    private async Task<Person?> GetPersonAsync(int personId)
    {
        // Direct query to Persons
        return null;
    }

    private async Task<Role?> GetDefaultRoleAsync()
    {
        // Get "Empleado" role as default
        return null;
    }

    private async Task AssignRoleAsync(int userId, int roleId)
    {
        // Assign role to user
        await Task.CompletedTask;
    }
}
