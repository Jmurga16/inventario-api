using Inventario.Domain.Entities;
using Inventario.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Catalogs
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<MovementType> MovementTypes => Set<MovementType>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<AuditAction> AuditActions => Set<AuditAction>();

    // Main entities
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
                
        modelBuilder.ApplyCustomConfigurations();
    }
}
