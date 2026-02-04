# Clean Architecture

## Introduccion

Este proyecto implementa Clean Architecture (Arquitectura Limpia), un patron de diseno propuesto por Robert C. Martin (Uncle Bob) que busca separar las responsabilidades del sistema en capas independientes.

## Principios Fundamentales

1. **Independencia de frameworks**: La logica de negocio no depende de librerias externas
2. **Testeable**: Las reglas de negocio se pueden probar sin UI, BD o servicios externos
3. **Independencia de UI**: La UI puede cambiar sin afectar el resto del sistema
4. **Independencia de BD**: Puedes cambiar SQL Server por PostgreSQL sin tocar la logica
5. **Independencia de agentes externos**: Las reglas de negocio no saben nada del mundo exterior

---

## Estructura del Proyecto

```
inventario-api/
│
├── Inventario.Domain/           ← Capa mas interna (entidades, interfaces)
├── Inventario.Application/      ← Casos de uso (servicios, DTOs, validadores)
├── Inventario.Infrastructure/   ← Implementaciones externas (EF, repositorios)
├── Inventario.Api/              ← Punto de entrada (controllers, middleware)
└── Inventario.UnitTests/        ← Pruebas unitarias
```

---

## Capas y Responsabilidades

### 1. Domain (Centro)

**Ubicacion:** `Inventario.Domain/`

**Responsabilidad:** Contiene las entidades del negocio y los contratos (interfaces). Es la capa mas estable y no depende de nada externo.

```
Inventario.Domain/
├── Entities/              # Entidades del negocio
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Person.cs
│   ├── Role.cs
│   ├── Product.cs
│   ├── Category.cs
│   ├── StockMovement.cs
│   ├── Notification.cs
│   ├── AuditLog.cs
│   └── Catalogs/
│       ├── DocumentType.cs
│       ├── MovementType.cs
│       ├── NotificationType.cs
│       └── AuditAction.cs
│
├── Interfaces/            # Contratos (que se hace, no como)
│   ├── Repositories/
│   │   ├── IBaseRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── IStockMovementRepository.cs
│   │   ├── INotificationRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Services/
│       ├── IJwtService.cs
│       ├── IPasswordHasher.cs
│       └── ICurrentUserService.cs
│
└── Exceptions/            # Excepciones de dominio
    ├── DomainException.cs
    ├── NotFoundException.cs
    └── ValidationException.cs
```

**Dependencias:** Ninguna (solo .NET base)

---

### 2. Application (Casos de Uso)

**Ubicacion:** `Inventario.Application/`

**Responsabilidad:** Contiene la logica de negocio, DTOs para comunicacion, servicios de aplicacion y **validadores con FluentValidation**.

```
Inventario.Application/
├── DTOs/                  # Data Transfer Objects
│   ├── Auth/
│   │   ├── LoginRequestDto.cs
│   │   ├── LoginResponseDto.cs
│   │   └── RegisterRequestDto.cs
│   ├── Products/
│   │   ├── ProductDto.cs
│   │   ├── CreateProductDto.cs
│   │   ├── UpdateProductDto.cs
│   │   └── ProductFilterDto.cs
│   ├── Categories/
│   │   ├── CategoryDto.cs
│   │   └── CreateCategoryDto.cs
│   ├── Stock/
│   │   ├── StockMovementDto.cs
│   │   └── CreateStockMovementDto.cs
│   ├── Notifications/
│   │   └── NotificationDto.cs
│   └── Common/
│       ├── ApiResponseDto.cs
│       └── PaginatedResultDto.cs
│
├── Interfaces/            # Contratos de servicios de aplicacion
│   ├── IAuthService.cs
│   ├── IProductService.cs
│   ├── ICategoryService.cs
│   ├── IStockService.cs
│   ├── INotificationService.cs
│   └── IReportService.cs
│
├── Services/              # Implementacion de casos de uso
│   ├── AuthService.cs
│   ├── ProductService.cs
│   ├── CategoryService.cs
│   ├── StockService.cs
│   ├── NotificationService.cs
│   └── ReportService.cs
│
├── Validators/            # Validaciones con FluentValidation
│   ├── Auth/
│   │   ├── LoginRequestValidator.cs
│   │   └── RegisterRequestValidator.cs
│   ├── Products/
│   │   ├── CreateProductValidator.cs
│   │   └── UpdateProductValidator.cs
│   ├── Categories/
│   │   └── CreateCategoryValidator.cs
│   └── Stock/
│       └── CreateStockMovementValidator.cs
│
└── DependencyInjection.cs # Registro de servicios y validadores
```

**Dependencias:** Solo `Inventario.Domain`

**Ejemplo de validador (FluentValidation):**
```csharp
namespace Inventario.Application.Validators.Products;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9-_]+$").WithMessage("SKU can only contain alphanumeric characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
    }
}
```

**Registro automatico de validadores:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IReportService, ReportService>();
        // ...

        // Validators - registra TODOS los validadores del assembly automaticamente
        services.AddValidatorsFromAssemblyContaining<IAuthService>();

        return services;
    }
}
```

---

### 3. Infrastructure (Implementaciones)

**Ubicacion:** `Inventario.Infrastructure/`

**Responsabilidad:** Implementa las interfaces definidas en Domain. Contiene todo lo relacionado con tecnologias externas: base de datos, servicios de email, APIs externas, etc.

```
Inventario.Infrastructure/
├── Persistence/           # Entity Framework
│   ├── ApplicationDbContext.cs
│   └── EntityConfigurations.cs  # Fluent API
│
├── Repositories/          # Implementacion de repositorios
│   ├── BaseRepository.cs
│   ├── ProductRepository.cs
│   ├── CategoryRepository.cs
│   ├── UserRepository.cs
│   ├── StockMovementRepository.cs
│   ├── NotificationRepository.cs
│   └── UnitOfWork.cs
│
├── Services/              # Servicios tecnicos
│   ├── JwtService.cs         # Generacion de tokens JWT
│   ├── PasswordHasher.cs     # Hash con BCrypt
│   └── CurrentUserService.cs # Usuario actual del request
│
└── DependencyInjection.cs # Registro de infraestructura
```

**Dependencias:** `Inventario.Domain`, `Inventario.Application`, Entity Framework, BCrypt, etc.

---

### 4. Api (Presentacion)

**Ubicacion:** `Inventario.Api/`

**Responsabilidad:** Punto de entrada de la aplicacion. Maneja HTTP, autenticacion, autorizacion, seguridad y serializacion.

```
Inventario.Api/
├── Controllers/           # Endpoints (nombres en SINGULAR)
│   ├── AuthController.cs
│   ├── ProductController.cs
│   ├── CategoryController.cs
│   ├── StockController.cs
│   ├── NotificationController.cs
│   └── ReportController.cs
│
├── Middleware/            # Middleware personalizado
│   ├── ExceptionMiddleware.cs      # Manejo global de errores
│   └── SecurityHeadersMiddleware.cs # Headers de seguridad OWASP
│
├── Program.cs             # Entry point y configuracion
├── appsettings.json
└── appsettings.Development.json
```

**Dependencias:** Todas las capas (para registrar servicios)

**Ejemplo de controller (nombre en singular):**
```csharp
namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]  // Ruta: /api/product
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductDto dto)  // Validacion automatica con FluentValidation
    {
        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponse<ProductDto>.Ok(product, "Product created"));
    }
}
```

---

## Seguridad (OWASP)

### Middleware de Security Headers

```csharp
public class SecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Prevenir MIME sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Prevenir clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // HSTS
        context.Response.Headers.Append("Strict-Transport-Security",
            "max-age=31536000; includeSubDomains");

        // Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; frame-ancestors 'none';");

        await _next(context);
    }
}
```

### Rate Limiting

```csharp
// Program.cs
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        // General: 100 requests/minuto
        new RateLimitRule { Endpoint = "*", Period = "1m", Limit = 100 },

        // Auth: 10 requests/minuto (prevenir fuerza bruta)
        new RateLimitRule { Endpoint = "*:/api/auth/*", Period = "1m", Limit = 10 }
    };
});
```

---

## Flujo de Dependencias

```
┌─────────────────────────────────────────────────────────────────┐
│                           API                                   │
│          (Controllers, Middleware, Program.cs)                  │
│                           │                                     │
│                           ▼                                     │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    APPLICATION                           │   │
│  │         (Services, DTOs, Validators)                     │   │
│  │                         │                                │   │
│  │                         ▼                                │   │
│  │  ┌─────────────────────────────────────────────────┐    │   │
│  │  │                   DOMAIN                         │    │   │
│  │  │       (Entities, Interfaces, Exceptions)         │    │   │
│  │  │                                                  │    │   │
│  │  └─────────────────────────────────────────────────┘    │   │
│  │                         ▲                                │   │
│  └─────────────────────────┼────────────────────────────────┘   │
│                            │                                     │
│  ┌─────────────────────────┴────────────────────────────────┐   │
│  │                  INFRASTRUCTURE                           │   │
│  │    (Repositories, DbContext, JWT, PasswordHasher)         │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**Regla de Dependencia:** Las flechas siempre apuntan hacia adentro. Domain no conoce a nadie. Application conoce a Domain. Infrastructure implementa interfaces de Domain.

---

## Inyeccion de Dependencias

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Registrar capas
builder.Services.AddApplication();      // Services + Validators
builder.Services.AddInfrastructure(builder.Configuration);  // Repos + DB

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

// FluentValidation auto-validation
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

// Middleware pipeline (orden importante)
app.UseSecurityHeaders();    // 1. Security headers
app.UseIpRateLimiting();     // 2. Rate limiting
app.UseMiddleware<ExceptionMiddleware>();  // 3. Exception handling
app.UseAuthentication();     // 4. Auth
app.UseAuthorization();      // 5. Authorization
app.MapControllers();        // 6. Endpoints
```

---

## Beneficios de esta Arquitectura

| Beneficio | Descripcion |
|-----------|-------------|
| **Testeable** | Puedes mockear interfaces y probar logica sin BD |
| **Mantenible** | Cambios en una capa no afectan otras |
| **Escalable** | Facil agregar nuevas funcionalidades |
| **Flexible** | Puedes cambiar tecnologias sin tocar el negocio |
| **Organizado** | Cada archivo tiene un lugar claro |
| **Seguro** | Validaciones centralizadas, headers OWASP, rate limiting |

---

## Convenciones del Proyecto

| Elemento | Convencion | Ejemplo |
|----------|------------|---------|
| Controllers | Singular | `ProductController`, `CategoryController` |
| Rutas API | Singular | `/api/product`, `/api/category` |
| DTOs | Sufijo Dto | `CreateProductDto`, `ProductDto` |
| Validators | Sufijo Validator | `CreateProductValidator` |
| Services | Sufijo Service | `ProductService`, `AuthService` |
| Repositories | Sufijo Repository | `ProductRepository` |

---

## Referencias

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft - Clean Architecture with ASP.NET Core](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [OWASP Security Headers](https://owasp.org/www-project-secure-headers/)
