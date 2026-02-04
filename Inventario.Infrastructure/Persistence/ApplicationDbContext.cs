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
    public DbSet<DocumentType> DocumentType => Set<DocumentType>();
    public DbSet<MovementType> MovementType => Set<MovementType>();
    public DbSet<NotificationType> NotificationType => Set<NotificationType>();
    public DbSet<AuditAction> AuditAction => Set<AuditAction>();

    // Main entities
    public DbSet<Person> Person => Set<Person>();
    public DbSet<User> User => Set<User>();
    public DbSet<Role> Role => Set<Role>();
    public DbSet<UserRole> UserRole => Set<UserRole>();
    public DbSet<Category> Category => Set<Category>();
    public DbSet<Product> Product => Set<Product>();
    public DbSet<StockMovement> StockMovement => Set<StockMovement>();
    public DbSet<Notification> Notification => Set<Notification>();
    public DbSet<AuditLog> AuditLog => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
                
        modelBuilder.ApplyCustomConfigurations();
    }
}
