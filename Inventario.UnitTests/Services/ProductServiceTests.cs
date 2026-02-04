using FluentAssertions;
using Inventario.Application.DTOs.Products;
using Inventario.Application.Interfaces;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;
using Moq;

namespace Inventario.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<INotificationService>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _notificationServiceMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            SKU = "PROD-001",
            Name = "Test Product",
            CategoryId = 1,
            UnitPrice = 99.99m,
            Quantity = 10,
            MinStock = 5,
            IsActive = true
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.SKU.Should().Be("PROD-001");
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingProduct_ReturnsNull()
    {
        // Arrange
        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, SKU = "PROD-001", Name = "Product 1", Quantity = 10, MinStock = 5 },
            new() { Id = 2, SKU = "PROD-002", Name = "Product 2", Quantity = 20, MinStock = 5 }
        };

        _productRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "NEW-001",
            Name = "New Product",
            CategoryId = 1,
            UnitPrice = 49.99m,
            Quantity = 15,
            MinStock = 5
        };

        _productRepositoryMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.SKU.Should().Be("NEW-001");
        result.Name.Should().Be("New Product");

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingSku_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "EXISTING-SKU",
            Name = "Product",
            CategoryId = 1
        };

        _productRepositoryMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _productService.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingCategory_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "NEW-001",
            Name = "Product",
            CategoryId = 999
        };

        _productRepositoryMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _productService.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithNegativeQuantity_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "NEW-001",
            Name = "Product",
            CategoryId = 1,
            Quantity = -5
        };

        _productRepositoryMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _productService.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithLowStock_TriggersNotification()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "LOW-001",
            Name = "Low Stock Product",
            CategoryId = 1,
            UnitPrice = 10m,
            Quantity = 3,
            MinStock = 5
        };

        _productRepositoryMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _productService.CreateAsync(dto);

        // Assert
        _notificationServiceMock.Verify(
            n => n.CreateLowStockNotificationAsync(It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            SKU = "PROD-001",
            Name = "Old Name",
            CategoryId = 1
        };

        var dto = new UpdateProductDto
        {
            Name = "New Name",
            CategoryId = 1,
            UnitPrice = 59.99m,
            MinStock = 10,
            IsActive = true
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingProduct);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.UpdateAsync(1, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        var dto = new UpdateProductDto { Name = "Test", CategoryId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _productService.UpdateAsync(999, dto));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_SetsInactive()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            SKU = "PROD-001",
            Name = "Product",
            IsActive = true
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(product);

        // Act
        await _productService.DeleteAsync(1);

        // Assert
        product.IsActive.Should().BeFalse();
        _productRepositoryMock.Verify(r => r.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _productService.DeleteAsync(999));
    }

    #endregion

    #region GetLowStockAsync Tests

    [Fact]
    public async Task GetLowStockAsync_ReturnsLowStockProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, SKU = "LOW-001", Name = "Low 1", Quantity = 2, MinStock = 5 },
            new() { Id = 2, SKU = "LOW-002", Name = "Low 2", Quantity = 0, MinStock = 5 }
        };

        _productRepositoryMock.Setup(r => r.GetLowStockProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetLowStockAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.IsLowStock || p.IsOutOfStock).Should().BeTrue();
    }

    #endregion
}
