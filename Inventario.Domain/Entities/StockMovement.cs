namespace Inventario.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public int MovementTypeId { get; set; }
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reason { get; set; }
    public string? Reference { get; set; }
    public int UserId { get; set; }
}
