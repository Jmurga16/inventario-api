using Inventario.Domain.Entities;

namespace Inventario.Domain.Interfaces.Repositories;

public interface IStockMovementRepository : IBaseRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
    Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<StockMovement>> GetByUserIdAsync(int userId);
}
