using Inventario.Domain.Entities;

namespace Inventario.Domain.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRolesAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    Task<Person?> GetPersonByIdAsync(int personId);
    Task<Person> AddPersonAsync(Person person);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task AddUserRoleAsync(int userId, int roleId, int? assignedBy = null);
    Task UpdateLastLoginAsync(int userId);
    Task<IEnumerable<int>> GetUserIdsByRoleAsync(string roleName);
}
