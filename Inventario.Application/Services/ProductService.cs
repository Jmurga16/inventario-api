using Inventario.Application.DTOs.Products;
using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Domain.Interfaces.Repositories;

namespace Inventario.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ProductService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return null;

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
        return MapToDto(product, category?.Name);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = (await _unitOfWork.Products.GetAllAsync()).ToList();
        var categoryNames = await GetCategoryNamesAsync(products);
        return products.Select(p => MapToDto(p, categoryNames.GetValueOrDefault(p.CategoryId)));
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter)
    {
        var products = await _unitOfWork.Products.SearchAsync(
            filter.Search,
            filter.CategoryId,
            filter.IsActive);

        var productList = products.ToList();

        if (filter.LowStockOnly == true)
        {
            productList = productList.Where(p => p.Quantity < p.MinStock).ToList();
        }

        var categoryNames = await GetCategoryNamesAsync(productList);
        return productList.Select(p => MapToDto(p, categoryNames.GetValueOrDefault(p.CategoryId)));
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockAsync()
    {
        var products = (await _unitOfWork.Products.GetLowStockProductsAsync()).ToList();
        var categoryNames = await GetCategoryNamesAsync(products);
        return products.Select(p => MapToDto(p, categoryNames.GetValueOrDefault(p.CategoryId)));
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await _unitOfWork.Products.SkuExistsAsync(dto.SKU))
            throw new ValidationException("SKU already exists");

        if (!await _unitOfWork.Categories.ExistsAsync(dto.CategoryId))
            throw new NotFoundException("Category", dto.CategoryId);

        if (dto.Quantity < 0)
            throw new ValidationException("Quantity cannot be negative");

        var product = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            UnitPrice = dto.UnitPrice,
            Cost = dto.Cost,
            Quantity = dto.Quantity,
            MinStock = dto.MinStock,
            MaxStock = dto.MaxStock,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        if (product.Quantity < product.MinStock)
        {
            await _notificationService.CreateLowStockNotificationAsync(product.Id);
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
        return MapToDto(product, category?.Name);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("Product", id);

        if (!await _unitOfWork.Categories.ExistsAsync(dto.CategoryId))
            throw new NotFoundException("Category", dto.CategoryId);

        var previousQuantity = product.Quantity;
        var previousMinStock = product.MinStock;
        var wasLowStock = previousQuantity < previousMinStock;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.Quantity = dto.Quantity;
        product.UnitPrice = dto.UnitPrice;
        product.Cost = dto.Cost;
        product.MinStock = dto.MinStock;
        product.MaxStock = dto.MaxStock;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Create notification if stock dropped below minimum
        var isNowLowStock = product.Quantity < product.MinStock;
        if (isNowLowStock && !wasLowStock)
        {
            await _notificationService.CreateLowStockNotificationAsync(product.Id);
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
        return MapToDto(product, category?.Name);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("Product", id);

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Dictionary<int, string>> GetCategoryNamesAsync(List<Product> products)
    {
        if (!products.Any()) return new Dictionary<int, string>();

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c.Name);
    }

    private static ProductDto MapToDto(Product product, string? categoryName = null)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            UnitPrice = product.UnitPrice,
            Cost = product.Cost,
            Quantity = product.Quantity,
            MinStock = product.MinStock,
            MaxStock = product.MaxStock,
            IsActive = product.IsActive,
            IsLowStock = product.IsLowStock,
            IsOutOfStock = product.IsOutOfStock,
            CreatedAt = product.CreatedAt
        };
    }
}
