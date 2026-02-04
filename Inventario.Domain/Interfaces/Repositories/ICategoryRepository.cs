using Inventario.Domain.Entities;

namespace Inventario.Domain.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
}
