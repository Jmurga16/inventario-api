using FluentAssertions;
using Inventario.Application.DTOs.Categories;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;
using Moq;

namespace Inventario.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ReturnsCategoryDto()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices",
            IsActive = true
        };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingCategory_ReturnsNull()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", IsActive = true },
            new() { Id = 2, Name = "Clothing", IsActive = true },
            new() { Id = 3, Name = "Inactive", IsActive = false }
        };

        _categoryRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region GetActiveAsync Tests

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var activeCategories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", IsActive = true },
            new() { Id = 2, Name = "Clothing", IsActive = true }
        };

        _categoryRepositoryMock.Setup(r => r.GetActiveCategoriesAsync())
            .ReturnsAsync(activeCategories);

        // Act
        var result = await _categoryService.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.IsActive).Should().BeTrue();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "New Category",
            Description = "Description"
        };

        _categoryRepositoryMock.Setup(r => r.NameExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Category");
        result.IsActive.Should().BeTrue();

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingName_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Existing Category"
        };

        _categoryRepositoryMock.Setup(r => r.NameExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _categoryService.CreateAsync(dto));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedCategory()
    {
        // Arrange
        var existingCategory = new Category
        {
            Id = 1,
            Name = "Old Name",
            IsActive = true
        };

        var dto = new CreateCategoryDto
        {
            Name = "New Name",
            Description = "New Description"
        };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingCategory);
        _categoryRepositoryMock.Setup(r => r.NameExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.UpdateAsync(1, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");

        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingCategory_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var dto = new CreateCategoryDto { Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _categoryService.UpdateAsync(999, dto));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ThrowsValidationException()
    {
        // Arrange
        var existingCategory = new Category { Id = 1, Name = "Original" };

        var dto = new CreateCategoryDto { Name = "Duplicate Name" };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingCategory);
        _categoryRepositoryMock.Setup(r => r.NameExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _categoryService.UpdateAsync(1, dto));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_SetsInactive()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Category",
            IsActive = true
        };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        await _categoryService.DeleteAsync(1);

        // Assert
        category.IsActive.Should().BeFalse();
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(category), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingCategory_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _categoryService.DeleteAsync(999));
    }

    #endregion
}
