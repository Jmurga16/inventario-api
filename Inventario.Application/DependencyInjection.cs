using FluentValidation;
using Inventario.Application.Interfaces;
using Inventario.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<IAuthService>();

        return services;
    }
}
