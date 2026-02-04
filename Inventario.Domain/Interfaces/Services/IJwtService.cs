using Inventario.Domain.Entities;

namespace Inventario.Domain.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    int? ValidateTokenAndGetUserId(string token);
}
