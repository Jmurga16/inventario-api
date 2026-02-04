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
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter)
    {
        var products = await _unitOfWork.Products.SearchAsync(
            filter.Search,
            filter.CategoryId,
            filter.IsActive);

        if (filter.LowStockOnly == true)
        {
            products = products.Where(p => p.Quantity < p.MinStock);
        }

        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockAsync()
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync();
        return products.Select(MapToDto);
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

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("Product", id);

        if (!await _unitOfWork.Categories.ExistsAsync(dto.CategoryId))
            throw new NotFoundException("Category", dto.CategoryId);

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.UnitPrice = dto.UnitPrice;
        product.Cost = dto.Cost;
        product.MinStock = dto.MinStock;
        product.MaxStock = dto.MaxStock;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(product);
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

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
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
