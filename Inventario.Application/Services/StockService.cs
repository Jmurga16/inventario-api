using Inventario.Application.DTOs.Stock;
using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;
using Inventario.Domain.Interfaces.Services;

namespace Inventario.Application.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public StockService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<StockMovementDto> AddMovementAsync(CreateStockMovementDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new NotFoundException("Product", dto.ProductId);

        var previousStock = product.Quantity;
        var newStock = CalculateNewStock(previousStock, dto.MovementTypeId, dto.Quantity);

        if (newStock < 0)
            throw new ValidationException("Insufficient stock for this operation");

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            MovementTypeId = dto.MovementTypeId,
            Quantity = dto.Quantity,
            PreviousStock = previousStock,
            NewStock = newStock,
            Reason = dto.Reason,
            Reference = dto.Reference,
            UserId = _currentUser.UserId ?? throw new UnauthorizedException()
        };

        product.Quantity = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StockMovements.AddAsync(movement);
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        if (newStock < product.MinStock)
        {
            await _notificationService.CreateLowStockNotificationAsync(product.Id);
        }

        return MapToDto(movement);
    }

    public async Task<IEnumerable<StockMovementDto>> GetByProductIdAsync(int productId)
    {
        var movements = await _unitOfWork.StockMovements.GetByProductIdAsync(productId);
        return movements.Select(MapToDto);
    }

    public async Task<IEnumerable<StockMovementDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var movements = await _unitOfWork.StockMovements.GetByDateRangeAsync(startDate, endDate);
        return movements.Select(MapToDto);
    }

    private static int CalculateNewStock(int currentStock, int movementTypeId, int quantity)
    {
        return movementTypeId switch
        {
            1 => currentStock + quantity,  // IN
            2 => currentStock - quantity,  // OUT
            3 => quantity,                  // ADJUSTMENT (sets absolute value)
            _ => currentStock
        };
    }

    private static StockMovementDto MapToDto(StockMovement movement)
    {
        return new StockMovementDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            MovementTypeId = movement.MovementTypeId,
            Quantity = movement.Quantity,
            PreviousStock = movement.PreviousStock,
            NewStock = movement.NewStock,
            Reason = movement.Reason,
            Reference = movement.Reference,
            UserId = movement.UserId,
            CreatedAt = movement.CreatedAt
        };
    }
}
