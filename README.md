# Inventario API

Sistema de gestión de inventarios para tienda en línea, desarrollado con .NET 8 y Clean Architecture.

## Tecnologías

| Capa | Tecnología |
|------|------------|
| Backend | .NET 8, C# |
| Base de datos | SQL Server |
| Autenticación | JWT |
| ORM | Entity Framework Core |
| Validación | FluentValidation |
| Testing | xUnit, Moq, FluentAssertions |
| Rate Limiting | AspNetCoreRateLimit |
| PDF Generation | QuestPDF |
| Contenedores | Docker |

## Estructura del Proyecto

```
inventario-api/
├── Inventario.Domain/           # Entidades, interfaces, excepciones
├── Inventario.Application/      # Lógica de negocio, DTOs, servicios, validadores
├── Inventario.Infrastructure/   # EF Core, repositorios, servicios externos
├── Inventario.Api/              # Controllers, middleware, configuración
├── Inventario.UnitTests/        # Pruebas unitarias
├── scripts/                     # Scripts SQL
└── docs/                        # Documentación
```

## Requisitos Previos

### Opción 1: Desarrollo Local
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express o superior)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

### Opción 2: Docker
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## Inicio Rápido con Docker

La forma más fácil de ejecutar el proyecto completo:

```bash
# Clonar e iniciar
cd inventario-api
docker-compose up -d

# Verificar que los contenedores estén corriendo
docker-compose ps
```

Esto levanta:
- **SQL Server** en puerto `1433`
- **API** en puerto `5000` → http://localhost:5000

Para ver logs:
```bash
docker-compose logs -f api
```

Para detener:
```bash
docker-compose down
```

Para eliminar todo (incluyendo datos):
```bash
docker-compose down -v
```

---

## Configuración Manual (Sin Docker)

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd inventario-api
```

### 2. Configurar la base de datos

Ejecuta los scripts en SQL Server en este orden:

```bash
scripts/001_schema.sql    # Estructura de tablas
scripts/002_seed.sql      # Datos iniciales
```

### 3. Configurar connection string

Edita `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InventarioDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Ejecutar la aplicación

```bash
cd Inventario.Api
dotnet run
```

La API estará disponible en: `https://localhost:7001` o `http://localhost:5001`

---

## Endpoints Principales

### Autenticación
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/register` | Registrar usuario |

### Productos
| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/product` | Listar productos | Auth |
| GET | `/api/product/{id}` | Obtener producto | Auth |
| GET | `/api/product/search` | Buscar productos | Auth |
| GET | `/api/product/low-stock` | Productos con stock bajo | Auth |
| POST | `/api/product` | Crear producto | Admin |
| PUT | `/api/product/{id}` | Actualizar producto | Admin |
| DELETE | `/api/product/{id}` | Eliminar producto | Admin |

### Categorías
| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/category` | Listar categorías | Auth |
| GET | `/api/category/active` | Categorías activas | Auth |
| GET | `/api/category/{id}` | Obtener categoría | Auth |
| POST | `/api/category` | Crear categoría | Admin |
| PUT | `/api/category/{id}` | Actualizar categoría | Admin |
| DELETE | `/api/category/{id}` | Eliminar categoría | Admin |

### Stock
| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| POST | `/api/stock/movement` | Registrar movimiento | Admin |
| GET | `/api/stock/product/{id}` | Movimientos por producto | Auth |
| GET | `/api/stock/history` | Historial por fechas | Auth |

### Notificaciones
| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/notification` | Mis notificaciones | Auth |
| GET | `/api/notification/unread` | No leídas | Auth |
| GET | `/api/notification/unread/count` | Contador | Auth |
| PUT | `/api/notification/{id}/read` | Marcar leída | Auth |
| PUT | `/api/notification/read-all` | Marcar todas | Auth |

### Reportes
| Método | Endpoint | Descripción | Rol |
|--------|----------|-------------|-----|
| GET | `/api/report/low-stock` | Datos JSON | Admin |
| GET | `/api/report/low-stock/pdf` | Descargar PDF | Admin |

---

## Seguridad Implementada (OWASP)

### Protecciones Activas

| Protección | Descripción |
|------------|-------------|
| **Validación de Entradas** | FluentValidation en todos los DTOs |
| **SQL Injection** | Entity Framework con queries parametrizadas |
| **Rate Limiting** | 100 req/min general, 10 req/min en auth |
| **Security Headers** | X-Content-Type-Options, X-Frame-Options, CSP, HSTS |
| **JWT Authentication** | Tokens con expiración y validación de firma |
| **Role-Based Authorization** | Admin y Empleado con permisos específicos |
| **CORS** | Configuración restrictiva por entorno |
| **Password Hashing** | BCrypt con salt automático |

### Headers de Seguridad

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains
Referrer-Policy: strict-origin-when-cross-origin
Content-Security-Policy: default-src 'self'; frame-ancestors 'none';
Permissions-Policy: camera=(), microphone=(), geolocation=()
```

---

## Pruebas Unitarias

El proyecto incluye pruebas unitarias con xUnit, Moq y FluentAssertions.

### Ejecutar tests
```bash
dotnet test
```

### Cobertura de tests
```
Inventario.UnitTests/
├── Services/
│   ├── AuthServiceTests.cs        # 5 tests
│   ├── ProductServiceTests.cs     # 11 tests
│   ├── CategoryServiceTests.cs    # tests
│   └── StockServiceTests.cs       # tests
└── Validators/
    ├── LoginRequestValidatorTests.cs      # 6 tests
    ├── RegisterRequestValidatorTests.cs   # tests
    ├── CreateProductValidatorTests.cs     # 12 tests
    └── CreateStockMovementValidatorTests.cs # tests
```

---

## Documentación

- [Modelo de Datos](docs/modelo-datos.md)
- [Clean Architecture](docs/clean-architecture.md)

---

## Roles y Permisos

| Rol | Permisos |
|-----|----------|
| **Admin** | Acceso completo: CRUD productos, gestión usuarios, reportes PDF |
| **Empleado** | Solo lectura: ver productos, ver notificaciones |

---

## Credenciales por Defecto

| Email | Password | Rol |
|-------|----------|-----|
| admin@inventario.com | Admin123! | Admin |

---

## Scripts Disponibles

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar en desarrollo
dotnet run --project Inventario.Api

# Ejecutar con hot reload
dotnet watch --project Inventario.Api
```

---

## Docker Commands

```bash
# Construir imagen
docker build -t inventario-api .

# Iniciar con docker-compose
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Detener
docker-compose down

# Detener y eliminar volúmenes
docker-compose down -v

# Reconstruir
docker-compose up -d --build
```

---

## Validaciones Implementadas

### LoginRequest
- Email: requerido, formato válido, máx 256 caracteres
- Password: requerido, mín 6 caracteres, máx 100 caracteres

### RegisterRequest
- Email: requerido, formato válido, máx 256 caracteres
- Password: mín 8 caracteres, mayúscula, minúscula, número, carácter especial
- FirstName/LastName: requerido, solo letras, máx 100 caracteres
- Phone: formato válido (opcional)
- DocumentNumber: alfanumérico (opcional)

### CreateProduct
- SKU: requerido, alfanumérico, máx 50 caracteres
- Name: requerido, máx 200 caracteres
- UnitPrice: >= 0
- Quantity: >= 0 (no negativo)
- MinStock: >= 0
- MaxStock: > MinStock (si existe)

### CreateStockMovement
- ProductId: requerido, > 0
- MovementTypeId: 1-3 (IN, OUT, ADJUSTMENT)
- Quantity: > 0

---

## Reglas de Negocio

1. **Notificaciones de stock bajo**: Se generan cuando `Quantity < MinStock`
2. **Notificaciones de sin stock**: Se generan cuando `Quantity = 0`
3. **Cantidad no negativa**: No se permite crear productos con cantidad negativa
4. **SKU único**: Cada producto debe tener un SKU único
5. **Soft delete**: Los productos se desactivan en lugar de eliminarse

---

## Autor

**Jose Murga**

## Licencia

Este proyecto es parte de una prueba técnica para Touch Consulting.
