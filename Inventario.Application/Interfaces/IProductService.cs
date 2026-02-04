using Inventario.Application.DTOs.Products;

namespace Inventario.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter);
    Task<IEnumerable<ProductDto>> GetLowStockAsync();
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
}
