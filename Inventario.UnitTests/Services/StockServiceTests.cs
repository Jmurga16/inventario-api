using FluentAssertions;
using Inventario.Application.DTOs.Stock;
using Inventario.Application.Interfaces;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;
using Inventario.Domain.Interfaces.Services;
using Moq;

namespace Inventario.UnitTests.Services;

public class StockServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly StockService _stockService;

    public StockServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);

        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _stockService = new StockService(
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            _notificationServiceMock.Object);
    }

    #region AddMovementAsync - IN Tests

    [Fact]
    public async Task AddMovementAsync_WithInMovement_IncreasesStock()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            SKU = "PROD-001",
            Name = "Test Product",
            Quantity = 10,
            MinStock = 5
        };

        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.In,
            Quantity = 5,
            Reason = "Restock"
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _stockService.AddMovementAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.PreviousStock.Should().Be(10);
        result.NewStock.Should().Be(15);
        result.Quantity.Should().Be(5);

        product.Quantity.Should().Be(15);
        _stockMovementRepositoryMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Once);
    }

    #endregion

    #region AddMovementAsync - OUT Tests

    [Fact]
    public async Task AddMovementAsync_WithOutMovement_DecreasesStock()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Quantity = 10,
            MinStock = 5
        };

        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.Out,
            Quantity = 3,
            Reason = "Sale"
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _stockService.AddMovementAsync(dto);

        // Assert
        result.PreviousStock.Should().Be(10);
        result.NewStock.Should().Be(7);
        product.Quantity.Should().Be(7);
    }

    [Fact]
    public async Task AddMovementAsync_WithOutMovement_InsufficientStock_ThrowsValidationException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Quantity = 5,
            MinStock = 5
        };

        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.Out,
            Quantity = 10,
            Reason = "Sale"
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _stockService.AddMovementAsync(dto));
    }

    #endregion

    #region AddMovementAsync - ADJUSTMENT Tests

    [Fact]
    public async Task AddMovementAsync_WithAdjustment_SetsAbsoluteStock()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Quantity = 10,
            MinStock = 5
        };

        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.Adjustment,
            Quantity = 25,
            Reason = "Physical inventory count"
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _stockService.AddMovementAsync(dto);

        // Assert
        result.PreviousStock.Should().Be(10);
        result.NewStock.Should().Be(25);
        product.Quantity.Should().Be(25);
    }

    #endregion

    #region AddMovementAsync - Notifications Tests

    [Fact]
    public async Task AddMovementAsync_WhenStockBecomesLow_TriggersNotification()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Quantity = 10,
            MinStock = 5
        };

        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.Out,
            Quantity = 8,
            Reason = "Sale"
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        await _stockService.AddMovementAsync(dto);

        // Assert
        product.Quantity.Should().Be(2);
        _notificationServiceMock.Verify(
            n => n.CreateLowStockNotificationAsync(1),
            Times.Once);
    }

    #endregion

    #region AddMovementAsync - Validation Tests

    [Fact]
    public async Task AddMovementAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new CreateStockMovementDto
        {
            ProductId = 999,
            MovementTypeId = (int)MovementTypeEnum.In,
            Quantity = 5
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _stockService.AddMovementAsync(dto));
    }

    [Fact]
    public async Task AddMovementAsync_WithoutAuthenticatedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var product = new Product { Id = 1, Quantity = 10, MinStock = 5 };

        var dto = new CreateStockMovementDto
        {
            ProductId = 1,
            MovementTypeId = (int)MovementTypeEnum.In,
            Quantity = 5
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        _currentUserMock.Setup(c => c.UserId).Returns((int?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _stockService.AddMovementAsync(dto));
    }

    #endregion

    #region GetByProductIdAsync Tests

    [Fact]
    public async Task GetByProductIdAsync_ReturnsMovementsForProduct()
    {
        // Arrange
        var movements = new List<StockMovement>
        {
            new() { Id = 1, ProductId = 1, Quantity = 10, PreviousStock = 0, NewStock = 10 },
            new() { Id = 2, ProductId = 1, Quantity = 5, PreviousStock = 10, NewStock = 5 }
        };

        _stockMovementRepositoryMock.Setup(r => r.GetByProductIdAsync(1))
            .ReturnsAsync(movements);

        // Act
        var result = await _stockService.GetByProductIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsMovementsInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;

        var movements = new List<StockMovement>
        {
            new() { Id = 1, ProductId = 1, Quantity = 10, CreatedAt = DateTime.Today.AddDays(-5) },
            new() { Id = 2, ProductId = 2, Quantity = 5, CreatedAt = DateTime.Today.AddDays(-2) }
        };

        _stockMovementRepositoryMock.Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(movements);

        // Act
        var result = await _stockService.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion
}
