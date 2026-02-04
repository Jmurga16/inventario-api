using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence;

public static class EntityConfigurations
{
    public static void ApplyCustomConfigurations(this ModelBuilder modelBuilder)
    {        
        modelBuilder.Entity<User>().ToTable("User");
                
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Cost).HasPrecision(18, 2);
            entity.Ignore(x => x.IsLowStock);
            entity.Ignore(x => x.IsOutOfStock);
        });
                
        modelBuilder.Entity<Person>().Ignore(x => x.FullName);
    }
}
