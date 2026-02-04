# Inventario API

Sistema de gestión de inventarios para tienda en línea, desarrollado con .NET 8 y Clean Architecture.

## Tecnologías

| Capa | Tecnología |
|------|------------|
| Backend | .NET 8, C# |
| Base de datos | SQL Server |
| Autenticación | JWT |
| ORM | Entity Framework Core |
| Testing | xUnit |

## Estructura del Proyecto

```
inventario-api/
├── Inventario.Domain/           # Entidades, interfaces, excepciones
├── Inventario.Application/      # Lógica de negocio, DTOs, servicios
├── Inventario.Infrastructure/   # EF Core, repositorios, servicios externos
├── Inventario.Api/              # Controllers, middleware, configuración
├── Inventario.UnitTests/        # Pruebas unitarias
├── scripts/                     # Scripts SQL
└── docs/                        # Documentación
```

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express o superior)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

## Configuración Inicial

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

## Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/register` | Registrar usuario |
| GET | `/api/products` | Listar productos |
| POST | `/api/products` | Crear producto |
| PUT | `/api/products/{id}` | Actualizar producto |
| DELETE | `/api/products/{id}` | Eliminar producto |
| GET | `/api/categories` | Listar categorías |
| GET | `/api/notifications` | Listar notificaciones |
| GET | `/api/reports/low-stock` | Reporte PDF de stock bajo |

## Documentación

- [Modelo de Datos](docs/modelo-datos.md)
- [Clean Architecture](docs/clean-architecture.md)

## Roles y Permisos

| Rol | Permisos |
|-----|----------|
| **Admin** | Acceso completo: CRUD productos, gestión usuarios, reportes |
| **Empleado** | Solo lectura: ver productos, reportar inventario bajo |

## Credenciales por Defecto

| Email | Password | Rol |
|-------|----------|-----|
| admin@inventario.com | Admin123! | Admin |

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
```

## Autor

**Jose Murga**

## Licencia

Este proyecto es parte de una prueba técnica para Touch Consulting.
