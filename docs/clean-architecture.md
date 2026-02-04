# Clean Architecture

## Introducción

Este proyecto implementa Clean Architecture (Arquitectura Limpia), un patrón de diseño propuesto por Robert C. Martin (Uncle Bob) que busca separar las responsabilidades del sistema en capas independientes.

## Principios Fundamentales

1. **Independencia de frameworks**: La lógica de negocio no depende de librerías externas
2. **Testeable**: Las reglas de negocio se pueden probar sin UI, BD o servicios externos
3. **Independencia de UI**: La UI puede cambiar sin afectar el resto del sistema
4. **Independencia de BD**: Puedes cambiar SQL Server por PostgreSQL sin tocar la lógica
5. **Independencia de agentes externos**: Las reglas de negocio no saben nada del mundo exterior

---

## Estructura del Proyecto

```
inventario-api/
│
├── Inventario.Domain/           ← Capa más interna
├── Inventario.Application/      ← Casos de uso
├── Inventario.Infrastructure/   ← Implementaciones externas
├── Inventario.Api/              ← Punto de entrada
└── Inventario.UnitTests/        ← Pruebas
```

---

## Capas y Responsabilidades

### 1. Domain (Centro)

**Ubicación:** `Inventario.Domain/`

**Responsabilidad:** Contiene las entidades del negocio y los contratos (interfaces). Es la capa más estable y no depende de nada externo.

```
Inventario.Domain/
├── Entities/              # Entidades del negocio
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Product.cs
│   └── Catalogs/
│       └── ...
│
├── Interfaces/            # Contratos (qué se hace, no cómo)
│   ├── Repositories/
│   │   ├── IBaseRepository.cs
│   │   ├── IProductRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Services/
│       ├── IJwtService.cs
│       └── IPasswordHasher.cs
│
└── Exceptions/            # Excepciones de dominio
    ├── DomainException.cs
    ├── NotFoundException.cs
    └── ValidationException.cs
```

**Dependencias:** Ninguna (solo .NET base)

**Ejemplo de entidad:**
```csharp
namespace Inventario.Domain.Entities;

public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; } = 5;

    // Lógica de dominio
    public bool IsLowStock => Quantity < MinStock;
}
```

**Ejemplo de interfaz:**
```csharp
namespace Inventario.Domain.Interfaces.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
}
```

---

### 2. Application (Casos de Uso)

**Ubicación:** `Inventario.Application/`

**Responsabilidad:** Contiene la lógica de negocio y orquesta las operaciones. Define DTOs para comunicación y servicios de aplicación.

```
Inventario.Application/
├── DTOs/                  # Data Transfer Objects
│   ├── Auth/
│   │   ├── LoginRequestDto.cs
│   │   └── LoginResponseDto.cs
│   ├── Products/
│   │   ├── ProductDto.cs
│   │   └── CreateProductDto.cs
│   └── Common/
│       └── ApiResponseDto.cs
│
├── Interfaces/            # Contratos de servicios de aplicación
│   ├── IAuthService.cs
│   └── IProductService.cs
│
├── Services/              # Implementación de casos de uso
│   ├── AuthService.cs
│   └── ProductService.cs
│
├── Validators/            # Validaciones (FluentValidation)
│   └── CreateProductValidator.cs
│
└── DependencyInjection.cs # Registro de servicios
```

**Dependencias:** Solo `Inventario.Domain`

**Ejemplo de servicio:**
```csharp
namespace Inventario.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;  // Interfaz de Domain

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Validar que SKU no exista
        if (await _unitOfWork.Products.SkuExistsAsync(dto.SKU))
            throw new ValidationException("SKU already exists");

        // Crear entidad
        var product = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            // ...
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(product);
    }
}
```

---

### 3. Infrastructure (Implementaciones)

**Ubicación:** `Inventario.Infrastructure/`

**Responsabilidad:** Implementa las interfaces definidas en Domain. Contiene todo lo relacionado con tecnologías externas: base de datos, servicios de email, APIs externas, etc.

```
Inventario.Infrastructure/
├── Persistence/           # Entity Framework
│   ├── ApplicationDbContext.cs
│   ├── Configurations/    # Fluent API
│   │   ├── ProductConfiguration.cs
│   │   └── UserConfiguration.cs
│   └── Migrations/
│
├── Repositories/          # Implementación de repositorios
│   ├── BaseRepository.cs
│   ├── ProductRepository.cs
│   └── UnitOfWork.cs
│
├── Services/              # Servicios técnicos
│   ├── JwtService.cs
│   ├── PasswordHasher.cs
│   └── PdfReportService.cs
│
└── DependencyInjection.cs # Registro de infraestructura
```

**Dependencias:** `Inventario.Domain`, `Inventario.Application`, Entity Framework, BCrypt, etc.

**Ejemplo de repositorio:**
```csharp
namespace Inventario.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive && p.Quantity < p.MinStock)
            .ToListAsync();
    }
}
```

---

### 4. Api (Presentación)

**Ubicación:** `Inventario.Api/`

**Responsabilidad:** Punto de entrada de la aplicación. Maneja HTTP, autenticación, autorización y serialización.

```
Inventario.Api/
├── Controllers/           # Endpoints
│   ├── AuthController.cs
│   └── ProductsController.cs
│
├── Middleware/            # Manejo de errores, logging
│   └── ExceptionMiddleware.cs
│
├── Extensions/            # Configuración
│   └── ServiceExtensions.cs
│
├── Program.cs             # Entry point
├── appsettings.json
└── appsettings.Development.json
```

**Dependencias:** Todas las capas (para registrar servicios)

**Ejemplo de controller:**
```csharp
namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
```

---

## Flujo de Dependencias

```
┌─────────────────────────────────────────────────────────────────┐
│                           API                                   │
│                      (Controllers)                              │
│                           │                                     │
│                           ▼                                     │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    APPLICATION                           │   │
│  │                 (Services, DTOs)                         │   │
│  │                         │                                │   │
│  │                         ▼                                │   │
│  │  ┌─────────────────────────────────────────────────┐    │   │
│  │  │                   DOMAIN                         │    │   │
│  │  │            (Entities, Interfaces)                │    │   │
│  │  │                                                  │    │   │
│  │  └─────────────────────────────────────────────────┘    │   │
│  │                         ▲                                │   │
│  └─────────────────────────┼────────────────────────────────┘   │
│                            │                                     │
│  ┌─────────────────────────┴────────────────────────────────┐   │
│  │                  INFRASTRUCTURE                           │   │
│  │         (Repositories, DbContext, Services)               │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**Regla de Dependencia:** Las flechas siempre apuntan hacia adentro. Domain no conoce a nadie. Application conoce a Domain. Infrastructure implementa interfaces de Domain.

---

## Inyección de Dependencias

Cada capa tiene su archivo `DependencyInjection.cs` que registra sus servicios:

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Registrar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
```

**Application/DependencyInjection.cs:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        // ...
        return services;
    }
}
```

**Infrastructure/DependencyInjection.cs:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
```

---

## Beneficios de esta Arquitectura

| Beneficio | Descripción |
|-----------|-------------|
| **Testeable** | Puedes mockear interfaces y probar lógica sin BD |
| **Mantenible** | Cambios en una capa no afectan otras |
| **Escalable** | Fácil agregar nuevas funcionalidades |
| **Flexible** | Puedes cambiar tecnologías sin tocar el negocio |
| **Organizado** | Cada archivo tiene un lugar claro |

---

## Referencias

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft - Clean Architecture with ASP.NET Core](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
