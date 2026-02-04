using Inventario.Domain.Entities;

namespace Inventario.Domain.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRolesAsync(int id);
    Task<bool> EmailExistsAsync(string email);
}
