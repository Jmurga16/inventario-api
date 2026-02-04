using Inventario.Domain.Entities;

namespace Inventario.Domain.Interfaces.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    Task<IEnumerable<Product>> GetOutOfStockProductsAsync();
    Task<IEnumerable<Product>> SearchAsync(string? name, int? categoryId, bool? isActive);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
}
