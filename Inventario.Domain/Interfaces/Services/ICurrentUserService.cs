namespace Inventario.Domain.Interfaces.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
