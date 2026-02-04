namespace Inventario.Domain.Entities;

public class Person : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed
    public string FullName => $"{FirstName} {LastName}";
}
