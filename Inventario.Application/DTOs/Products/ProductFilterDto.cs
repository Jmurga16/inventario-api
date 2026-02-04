namespace Inventario.Application.DTOs.Products;

public class ProductFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? LowStockOnly { get; set; }
}
