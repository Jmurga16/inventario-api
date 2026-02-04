using Inventario.Application.DTOs.Auth;
using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
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

        if (user == null)
            throw new UnauthorizedException("Invalid credentials");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedException("User is inactive");

        // Update last login
        await _unitOfWork.Users.UpdateLastLoginAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        // Get user data
        var roles = await _unitOfWork.Users.GetUserRolesAsync(user.Id);
        var person = await _unitOfWork.Users.GetPersonByIdAsync(user.PersonId);
        var token = _jwtService.GenerateToken(user, roles);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = person?.FullName ?? string.Empty,
            Token = token,
            Roles = roles
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate email doesn't exist
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            throw new ValidationException("Email already exists");

        // Begin transaction for atomicity
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Create person
            var person = new Person
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                DocumentTypeId = request.DocumentTypeId,
                DocumentNumber = request.DocumentNumber?.Trim(),
                Phone = request.Phone?.Trim()
            };

            await _unitOfWork.Users.AddPersonAsync(person);
            await _unitOfWork.SaveChangesAsync();

            // Create user
            var user = new User
            {
                PersonId = person.Id,
                Email = request.Email.ToLower().Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Assign default role (Empleado)
            var defaultRole = await _unitOfWork.Users.GetRoleByNameAsync(RoleNames.Empleado);
            if (defaultRole != null)
            {
                await _unitOfWork.Users.AddUserRoleAsync(user.Id, defaultRole.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            // Generate response
            var roles = await _unitOfWork.Users.GetUserRolesAsync(user.Id);
            var token = _jwtService.GenerateToken(user, roles);

            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = person.FullName,
                Token = token,
                Roles = roles
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
