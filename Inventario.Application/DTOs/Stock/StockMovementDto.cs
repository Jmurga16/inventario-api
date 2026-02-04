namespace Inventario.Application.DTOs.Stock;

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public int MovementTypeId { get; set; }
    public string? MovementTypeName { get; set; }
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reason { get; set; }
    public string? Reference { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
