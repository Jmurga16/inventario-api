using Inventario.Application.Interfaces;
using Inventario.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
