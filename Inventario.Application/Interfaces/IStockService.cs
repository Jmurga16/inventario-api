using Inventario.Application.DTOs.Stock;

namespace Inventario.Application.Interfaces;

public interface IStockService
{
    Task<StockMovementDto> AddMovementAsync(CreateStockMovementDto dto);
    Task<IEnumerable<StockMovementDto>> GetByProductIdAsync(int productId);
    Task<IEnumerable<StockMovementDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
