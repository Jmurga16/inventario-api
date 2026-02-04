# Modelo de Datos

## Diagrama de Entidades

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│ DocumentType │     │ MovementType │     │Notification- │     │ AuditAction  │
│   (Catalog)  │     │   (Catalog)  │     │    Type      │     │   (Catalog)  │
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                    │                    │                    │
       ▼                    │                    │                    │
┌──────────────┐            │                    │                    │
│    Person    │            │                    │                    │
└──────┬───────┘            │                    │                    │
       │                    │                    │                    │
       ▼                    │                    │                    │
┌──────────────┐            │                    │                    │
│     User     │◄───────────┼────────────────────┼────────────────────┤
└──────┬───────┘            │                    │                    │
       │                    │                    │                    │
       ▼                    ▼                    ▼                    ▼
┌────────────┐       ┌────────────┐       ┌────────────┐       ┌────────────┐
│  UserRole  │       │   Stock    │       │Notification│       │  AuditLog  │
└────────────┘       │  Movement  │       └────────────┘       └────────────┘
       │             └────────────┘              │
       ▼                    ▲                    │
┌────────────┐              │                    │
│    Role    │       ┌────────────┐              │
└────────────┘       │  Product   │◄─────────────┘
                     └────────────┘
                            ▲
                            │
                     ┌────────────┐
                     │  Category  │
                     └────────────┘
```

---

## Tablas de Catalogos

### DocumentType
Tipos de documento de identidad.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(10) | Codigo unico (DNI, RUC, CE, PASSPORT) |
| Name | NVARCHAR(50) | Nombre descriptivo |
| IsActive | BIT | Estado activo/inactivo |

### MovementType
Tipos de movimiento de inventario.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(20) | Codigo unico (IN, OUT, ADJUSTMENT) |
| Name | NVARCHAR(50) | Nombre descriptivo |
| Description | NVARCHAR(255) | Descripcion opcional |
| IsActive | BIT | Estado activo/inactivo |

**Valores:**
- `IN` (Id: 1) - Entrada de productos al inventario
- `OUT` (Id: 2) - Salida de productos del inventario
- `ADJUSTMENT` (Id: 3) - Ajuste manual por inventario fisico

### NotificationType
Tipos de notificacion del sistema.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(30) | Codigo unico |
| Name | NVARCHAR(50) | Nombre descriptivo |
| Description | NVARCHAR(255) | Descripcion opcional |
| IsActive | BIT | Estado activo/inactivo |

**Valores:**
- `SYSTEM` - Notificacion general del sistema
- `LOW_STOCK` - Producto por debajo del stock minimo
- `OUT_OF_STOCK` - Producto agotado

### AuditAction
Acciones de auditoria.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(20) | Codigo unico (CREATED, MODIFIED, DELETED) |
| Name | NVARCHAR(50) | Nombre descriptivo |
| IsActive | BIT | Estado activo/inactivo |

---

## Tablas Principales

### Person
Datos personales (separados de autenticacion).

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| FirstName | NVARCHAR(100) | Nombre |
| LastName | NVARCHAR(100) | Apellido |
| DocumentTypeId | INT | FK → DocumentType (opcional) |
| DocumentNumber | NVARCHAR(20) | Numero de documento |
| Phone | NVARCHAR(20) | Telefono |
| Address | NVARCHAR(500) | Direccion |
| BirthDate | DATE | Fecha de nacimiento |
| CreatedAt | DATETIME2 | Fecha de creacion |
| UpdatedAt | DATETIME2 | Fecha de actualizacion |

### User
Credenciales de autenticacion.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| PersonId | INT | FK → Person |
| Email | NVARCHAR(255) | Email unico |
| PasswordHash | NVARCHAR(500) | Hash BCrypt del password |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creacion |
| LastLoginAt | DATETIME2 | Ultimo inicio de sesion |

### Role
Roles del sistema.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Name | NVARCHAR(50) | Nombre unico (Admin, Empleado) |
| Description | NVARCHAR(255) | Descripcion del rol |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creacion |

### UserRole
Relacion muchos a muchos entre User y Role.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| UserId | INT | FK → User |
| RoleId | INT | FK → Role |
| AssignedAt | DATETIME2 | Fecha de asignacion |
| AssignedBy | INT | FK → User (quien asigno) |

### Category
Categorias de productos.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Name | NVARCHAR(100) | Nombre unico |
| Description | NVARCHAR(500) | Descripcion |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creacion |

### Product
Productos del inventario.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| SKU | NVARCHAR(50) | Codigo unico del producto |
| Name | NVARCHAR(200) | Nombre del producto |
| Description | NVARCHAR(1000) | Descripcion |
| CategoryId | INT | FK → Category |
| UnitPrice | DECIMAL(18,2) | Precio de venta |
| Cost | DECIMAL(18,2) | Precio de costo (opcional) |
| Quantity | INT | Cantidad en inventario |
| MinStock | INT | Stock minimo (default: 5) |
| MaxStock | INT | Stock maximo (opcional) |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creacion |
| UpdatedAt | DATETIME2 | Fecha de actualizacion |

**Constraints:**
- `Quantity >= 0`
- `UnitPrice >= 0`
- `Cost >= 0` (si tiene valor)
- `MinStock >= 0`

### StockMovement
Historial de movimientos de inventario.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| ProductId | INT | FK → Product |
| MovementTypeId | INT | FK → MovementType |
| Quantity | INT | Cantidad movida |
| PreviousStock | INT | Stock antes del movimiento |
| NewStock | INT | Stock despues del movimiento |
| Reason | NVARCHAR(500) | Razon del movimiento |
| Reference | NVARCHAR(100) | Referencia (factura, orden, etc.) |
| UserId | INT | FK → User (quien realizo) |
| CreatedAt | DATETIME2 | Fecha del movimiento |

### Notification
Notificaciones del sistema.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | INT | PK, Identity |
| UserId | INT | FK → User (destinatario) |
| ProductId | INT | FK → Product (opcional) |
| NotificationTypeId | INT | FK → NotificationType |
| Title | NVARCHAR(200) | Titulo |
| Message | NVARCHAR(1000) | Mensaje |
| IsRead | BIT | Leida o no |
| ReadAt | DATETIME2 | Fecha de lectura |
| CreatedAt | DATETIME2 | Fecha de creacion |

### AuditLog
Registro de auditoria del sistema.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| Id | BIGINT | PK, Identity |
| TableName | NVARCHAR(100) | Nombre de la tabla afectada |
| RecordId | INT | ID del registro afectado |
| AuditActionId | INT | FK → AuditAction |
| OldValues | NVARCHAR(MAX) | Valores anteriores (JSON) |
| NewValues | NVARCHAR(MAX) | Valores nuevos (JSON) |
| UserId | INT | FK → User (quien realizo) |
| IpAddress | NVARCHAR(45) | IP del cliente |
| UserAgent | NVARCHAR(500) | User-Agent del cliente |
| Message | NVARCHAR(500) | Mensaje adicional |
| CreatedAt | DATETIME2 | Fecha del registro |

---

## Indices

| Tabla | Indice | Campos |
|-------|--------|--------|
| Person | IX_Person_DocumentNumber | DocumentNumber |
| Person | IX_Person_FullName | LastName, FirstName |
| User | IX_User_Email | Email |
| User | IX_User_PersonId | PersonId |
| Product | IX_Product_SKU | SKU |
| Product | IX_Product_CategoryId | CategoryId |
| Product | IX_Product_Quantity | Quantity |
| StockMovement | IX_StockMovement_ProductId | ProductId |
| StockMovement | IX_StockMovement_CreatedAt | CreatedAt |
| Notification | IX_Notification_UserId | UserId |
| Notification | IX_Notification_IsRead | IsRead |
| AuditLog | IX_AuditLog_TableName | TableName |
| AuditLog | IX_AuditLog_CreatedAt | CreatedAt |

---

## Reglas de Negocio

1. **Stock bajo**: Cuando `Quantity < MinStock`, se genera notificacion `LOW_STOCK`
2. **Sin stock**: Cuando `Quantity = 0`, se genera notificacion `OUT_OF_STOCK`
3. **Cantidad no negativa**: No se permite `Quantity < 0`
4. **SKU unico**: Cada producto debe tener un SKU unico
5. **Email unico**: Cada usuario debe tener un email unico
6. **Documento unico**: Combinacion `DocumentTypeId + DocumentNumber` es unica

---

## Validaciones de Entrada (API)

Las siguientes validaciones se aplican automaticamente con **FluentValidation** antes de procesar cualquier request:

### LoginRequest
| Campo | Validacion |
|-------|------------|
| Email | Requerido, formato email valido, max 256 caracteres |
| Password | Requerido, min 6 caracteres, max 100 caracteres |

### RegisterRequest
| Campo | Validacion |
|-------|------------|
| Email | Requerido, formato email valido, max 256 caracteres |
| Password | Min 8 caracteres, 1 mayuscula, 1 minuscula, 1 numero, 1 caracter especial |
| FirstName | Requerido, solo letras, max 100 caracteres |
| LastName | Requerido, solo letras, max 100 caracteres |
| Phone | Formato valido (opcional), max 20 caracteres |
| DocumentNumber | Alfanumerico (opcional), max 20 caracteres |

### CreateProduct
| Campo | Validacion |
|-------|------------|
| SKU | Requerido, alfanumerico + guiones, max 50 caracteres |
| Name | Requerido, max 200 caracteres |
| Description | Max 1000 caracteres (opcional) |
| CategoryId | Requerido, > 0 |
| UnitPrice | >= 0, max 999,999,999.99 |
| Cost | >= 0 (opcional) |
| Quantity | >= 0 (no negativo) |
| MinStock | >= 0, max 1,000,000 |
| MaxStock | > MinStock (si existe) |

### UpdateProduct
| Campo | Validacion |
|-------|------------|
| Name | Requerido, max 200 caracteres |
| Description | Max 1000 caracteres (opcional) |
| CategoryId | Requerido, > 0 |
| UnitPrice | >= 0 |
| Cost | >= 0 (opcional) |
| MinStock | >= 0 |
| MaxStock | > MinStock (si existe) |

### CreateCategory
| Campo | Validacion |
|-------|------------|
| Name | Requerido, max 100 caracteres |
| Description | Max 500 caracteres (opcional) |

### CreateStockMovement
| Campo | Validacion |
|-------|------------|
| ProductId | Requerido, > 0 |
| MovementTypeId | Requerido, 1-3 (IN, OUT, ADJUSTMENT) |
| Quantity | > 0, max 1,000,000 |
| Reason | Max 500 caracteres (opcional) |
| Reference | Max 100 caracteres (opcional) |

---

## Respuestas de Error de Validacion

Cuando una validacion falla, la API retorna HTTP 400 con el siguiente formato:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

---

## Datos Iniciales (Seed)

### Roles
| Id | Name | Descripcion |
|----|------|-------------|
| 1 | Admin | Administrador con acceso completo |
| 2 | Empleado | Empleado con acceso limitado |

### Usuario por Defecto
| Email | Password | Rol |
|-------|----------|-----|
| admin@inventario.com | Admin123! | Admin |

### Categorias Iniciales
- Electronicos
- Ropa y Accesorios
- Hogar y Cocina
- Deportes
- Libros
- Juguetes
- Alimentos
- Salud y Belleza
