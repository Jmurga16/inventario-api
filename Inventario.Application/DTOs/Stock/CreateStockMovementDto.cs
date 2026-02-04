namespace Inventario.Application.DTOs.Stock;

public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public int MovementTypeId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Reference { get; set; }
}
