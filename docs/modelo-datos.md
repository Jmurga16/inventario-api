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

## Tablas de Catálogos

### DocumentType
Tipos de documento de identidad.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(10) | Código único (DNI, RUC, CE, PASSPORT) |
| Name | NVARCHAR(50) | Nombre descriptivo |
| IsActive | BIT | Estado activo/inactivo |

### MovementType
Tipos de movimiento de inventario.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(20) | Código único (IN, OUT, ADJUSTMENT) |
| Name | NVARCHAR(50) | Nombre descriptivo |
| Description | NVARCHAR(255) | Descripción opcional |
| IsActive | BIT | Estado activo/inactivo |

**Valores:**
- `IN` - Entrada de productos al inventario
- `OUT` - Salida de productos del inventario
- `ADJUSTMENT` - Ajuste manual por inventario físico

### NotificationType
Tipos de notificación del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(30) | Código único |
| Name | NVARCHAR(50) | Nombre descriptivo |
| Description | NVARCHAR(255) | Descripción opcional |
| IsActive | BIT | Estado activo/inactivo |

**Valores:**
- `SYSTEM` - Notificación general del sistema
- `LOW_STOCK` - Producto por debajo del stock mínimo
- `OUT_OF_STOCK` - Producto agotado

### AuditAction
Acciones de auditoría.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Code | NVARCHAR(20) | Código único (CREATED, MODIFIED, DELETED) |
| Name | NVARCHAR(50) | Nombre descriptivo |
| IsActive | BIT | Estado activo/inactivo |

---

## Tablas Principales

### Person
Datos personales (separados de autenticación).

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| FirstName | NVARCHAR(100) | Nombre |
| LastName | NVARCHAR(100) | Apellido |
| DocumentTypeId | INT | FK → DocumentType (opcional) |
| DocumentNumber | NVARCHAR(20) | Número de documento |
| Phone | NVARCHAR(20) | Teléfono |
| Address | NVARCHAR(500) | Dirección |
| BirthDate | DATE | Fecha de nacimiento |
| CreatedAt | DATETIME2 | Fecha de creación |
| UpdatedAt | DATETIME2 | Fecha de actualización |

### User
Credenciales de autenticación.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| PersonId | INT | FK → Person |
| Email | NVARCHAR(255) | Email único |
| PasswordHash | NVARCHAR(500) | Hash BCrypt del password |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creación |
| LastLoginAt | DATETIME2 | Último inicio de sesión |

### Role
Roles del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Name | NVARCHAR(50) | Nombre único (Admin, Empleado) |
| Description | NVARCHAR(255) | Descripción del rol |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creación |

### UserRole
Relación muchos a muchos entre User y Role.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| UserId | INT | FK → User |
| RoleId | INT | FK → Role |
| AssignedAt | DATETIME2 | Fecha de asignación |
| AssignedBy | INT | FK → User (quién asignó) |

### Category
Categorías de productos.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| Name | NVARCHAR(100) | Nombre único |
| Description | NVARCHAR(500) | Descripción |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creación |

### Product
Productos del inventario.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| SKU | NVARCHAR(50) | Código único del producto |
| Name | NVARCHAR(200) | Nombre del producto |
| Description | NVARCHAR(1000) | Descripción |
| CategoryId | INT | FK → Category |
| UnitPrice | DECIMAL(18,2) | Precio de venta |
| Cost | DECIMAL(18,2) | Precio de costo (opcional) |
| Quantity | INT | Cantidad en inventario |
| MinStock | INT | Stock mínimo (default: 5) |
| MaxStock | INT | Stock máximo (opcional) |
| IsActive | BIT | Estado activo/inactivo |
| CreatedAt | DATETIME2 | Fecha de creación |
| UpdatedAt | DATETIME2 | Fecha de actualización |

**Constraints:**
- `Quantity >= 0`
- `UnitPrice >= 0`
- `Cost >= 0` (si tiene valor)
- `MinStock >= 0`

### StockMovement
Historial de movimientos de inventario.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| ProductId | INT | FK → Product |
| MovementTypeId | INT | FK → MovementType |
| Quantity | INT | Cantidad movida |
| PreviousStock | INT | Stock antes del movimiento |
| NewStock | INT | Stock después del movimiento |
| Reason | NVARCHAR(500) | Razón del movimiento |
| Reference | NVARCHAR(100) | Referencia (factura, orden, etc.) |
| UserId | INT | FK → User (quién realizó) |
| CreatedAt | DATETIME2 | Fecha del movimiento |

### Notification
Notificaciones del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT | PK, Identity |
| UserId | INT | FK → User (destinatario) |
| ProductId | INT | FK → Product (opcional) |
| NotificationTypeId | INT | FK → NotificationType |
| Title | NVARCHAR(200) | Título |
| Message | NVARCHAR(1000) | Mensaje |
| IsRead | BIT | Leída o no |
| ReadAt | DATETIME2 | Fecha de lectura |
| CreatedAt | DATETIME2 | Fecha de creación |

### AuditLog
Registro de auditoría del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | BIGINT | PK, Identity |
| TableName | NVARCHAR(100) | Nombre de la tabla afectada |
| RecordId | INT | ID del registro afectado |
| AuditActionId | INT | FK → AuditAction |
| OldValues | NVARCHAR(MAX) | Valores anteriores (JSON) |
| NewValues | NVARCHAR(MAX) | Valores nuevos (JSON) |
| UserId | INT | FK → User (quién realizó) |
| IpAddress | NVARCHAR(45) | IP del cliente |
| UserAgent | NVARCHAR(500) | User-Agent del cliente |
| Message | NVARCHAR(500) | Mensaje adicional |
| CreatedAt | DATETIME2 | Fecha del registro |

---

## Índices

| Tabla | Índice | Campos |
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

1. **Stock bajo**: Cuando `Quantity < MinStock`, se genera notificación `LOW_STOCK`
2. **Sin stock**: Cuando `Quantity = 0`, se genera notificación `OUT_OF_STOCK`
3. **Cantidad no negativa**: No se permite `Quantity < 0`
4. **SKU único**: Cada producto debe tener un SKU único
5. **Email único**: Cada usuario debe tener un email único
6. **Documento único**: Combinación `DocumentTypeId + DocumentNumber` es única
