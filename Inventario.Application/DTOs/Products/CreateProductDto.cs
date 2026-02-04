namespace Inventario.Application.DTOs.Products;

public class CreateProductDto
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; } = 5;
    public int? MaxStock { get; set; }
}
