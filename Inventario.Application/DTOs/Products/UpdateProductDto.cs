namespace Inventario.Application.DTOs.Products;

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public int MinStock { get; set; }
    public int? MaxStock { get; set; }
    public bool IsActive { get; set; }
}
