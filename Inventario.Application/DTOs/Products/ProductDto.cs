namespace Inventario.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public int? MaxStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public DateTime CreatedAt { get; set; }
}
