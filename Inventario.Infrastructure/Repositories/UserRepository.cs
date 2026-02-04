using Inventario.Domain.Entities;
using Inventario.Domain.Interfaces.Repositories;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
    }

    public async Task<User?> GetByIdWithRolesAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        var roles = await (
            from ur in _context.UserRole
            join r in _context.Role on ur.RoleId equals r.Id
            where ur.UserId == userId && r.IsActive
            select r.Name
        ).ToListAsync();

        return roles;
    }

    public async Task<Person?> GetPersonByIdAsync(int personId)
    {
        return await _context.Person.FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<Person> AddPersonAsync(Person person)
    {
        await _context.Person.AddAsync(person);
        return person;
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Role
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower() && r.IsActive);
    }

    public async Task AddUserRoleAsync(int userId, int roleId, int? assignedBy = null)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };

        await _context.UserRole.AddAsync(userRole);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<int>> GetUserIdsByRoleAsync(string roleName)
    {
        var userIds = await (
            from ur in _context.UserRole
            join r in _context.Role on ur.RoleId equals r.Id
            join u in _context.User on ur.UserId equals u.Id
            where r.Name.ToLower() == roleName.ToLower() && r.IsActive && u.IsActive
            select ur.UserId
        ).Distinct().ToListAsync();

        return userIds;
    }
}
