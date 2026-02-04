namespace Inventario.Domain.Entities;

public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Cost { get; set; }
    public int Quantity { get; set; } = 0;
    public int MinStock { get; set; } = 5;
    public int? MaxStock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }

    // Computed
    public bool IsLowStock => Quantity < MinStock;
    public bool IsOutOfStock => Quantity == 0;
}
