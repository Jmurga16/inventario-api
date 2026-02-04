-- ============================================
-- Sistema de Gestión de Inventarios
-- Seed: 002_seed.sql
-- Datos iniciales del sistema
-- ============================================

-- ============================================
-- CATÁLOGOS
-- ============================================

-- Tipos de documento
INSERT INTO DocumentType (Code, Name, IsActive)
VALUES
    ('DNI', 'Documento Nacional de Identidad', 1),
    ('RUC', 'Registro Único de Contribuyente', 1),
    ('CE', 'Carnet de Extranjería', 1),
    ('PASSPORT', 'Pasaporte', 1);

-- Tipos de movimiento de inventario
INSERT INTO MovementType (Code, Name, Description, IsActive)
VALUES
    ('IN', 'Entrada', 'Ingreso de productos al inventario', 1),
    ('OUT', 'Salida', 'Salida de productos del inventario', 1),
    ('ADJUSTMENT', 'Ajuste', 'Ajuste manual por inventario físico o corrección', 1);

-- Tipos de notificación
INSERT INTO NotificationType (Code, Name, Description, IsActive)
VALUES
    ('SYSTEM', 'Sistema', 'Notificación general del sistema', 1),
	('LOW_STOCK', 'Stock Bajo', 'El producto está por debajo del stock mínimo', 1),
    ('OUT_OF_STOCK', 'Sin Stock', 'El producto está agotado', 1);    

-- Acciones de auditoría
INSERT INTO AuditAction (Code, Name, IsActive)
VALUES
    ('CREATED', 'Creado', 1),
    ('MODIFIED', 'Modificado', 1),
    ('DELETED', 'Eliminado', 1);

-- ============================================
-- ROLES
-- ============================================
INSERT INTO Role (Name, Description, IsActive)
VALUES
    ('Admin', 'Administrador con acceso completo al sistema', 1),
    ('Empleado', 'Acceso limitado: ver productos y reportar inventarios bajos', 1);

-- ============================================
-- PERSONA Y USUARIO ADMINISTRADOR
-- Password: Admin123!
-- Nota: El hash debe generarse con BCrypt en la aplicación
-- ============================================
INSERT INTO Person (FirstName, LastName, DocumentTypeId, DocumentNumber, Phone)
VALUES
    ('Administrador', 'Sistema', 1, '00000000', NULL);

INSERT INTO [User] (PersonId, Email, PasswordHash, IsActive)
VALUES
    (1, 'admin@inventario.com', '$2a$11$HC.Ce7dxvxdfXvZYuDvmdOkdjRLCiTUDTPGJczx31iALGrEXxTuv6', 1);

-- Asignar rol Admin al usuario administrador
INSERT INTO UserRole (UserId, RoleId, AssignedBy)
VALUES
    (1, 1, NULL);

-- ============================================
-- CATEGORÍAS DE PRODUCTOS
-- ============================================
INSERT INTO Category (Name, Description, IsActive)
VALUES
    ('Electrónica', 'Dispositivos electrónicos, gadgets y accesorios', 1),
    ('Ropa', 'Vestimenta y accesorios de moda', 1),
    ('Hogar', 'Artículos para el hogar y decoración', 1),
    ('Deportes', 'Equipamiento y ropa deportiva', 1),
    ('Libros', 'Libros físicos y material de lectura', 1),
    ('Juguetes', 'Juguetes y juegos para todas las edades', 1),
    ('Alimentos', 'Productos alimenticios no perecederos', 1),
    ('Salud', 'Productos de salud y cuidado personal', 1);

-- ============================================
-- PRODUCTOS DE EJEMPLO
-- ============================================
INSERT INTO Product (SKU, Name, Description, CategoryId, UnitPrice, Cost, Quantity, MinStock, MaxStock, IsActive)
VALUES
    -- Electrónica (CategoryId = 1)
    ('ELEC-001', 'Audífonos Bluetooth', 'Audífonos inalámbricos con cancelación de ruido', 1, 89.99, 45.00, 25, 5, 100, 1),
    ('ELEC-002', 'Cargador USB-C', 'Cargador rápido 65W compatible con múltiples dispositivos', 1, 35.99, 18.00, 50, 10, 200, 1),
    ('ELEC-003', 'Mouse Inalámbrico', 'Mouse ergonómico con receptor USB', 1, 29.99, 12.00, 3, 5, 150, 1),
    ('ELEC-004', 'Teclado Mecánico', 'Teclado gaming con switches rojos', 1, 149.99, 75.00, 15, 5, 50, 1),

    -- Ropa (CategoryId = 2)
    ('ROPA-001', 'Camiseta Básica', 'Camiseta algodón 100%, varios colores', 2, 19.99, 8.00, 100, 20, 500, 1),
    ('ROPA-002', 'Jeans Clásico', 'Pantalón jean corte recto', 2, 59.99, 25.00, 45, 10, 200, 1),
    ('ROPA-003', 'Zapatillas Deportivas', 'Zapatillas running con amortiguación', 2, 89.99, 40.00, 2, 5, 100, 1),

    -- Hogar (CategoryId = 3)
    ('HOGR-001', 'Lámpara LED Escritorio', 'Lámpara ajustable con 3 niveles de luz', 3, 45.99, 20.00, 30, 5, 100, 1),
    ('HOGR-002', 'Set de Toallas', 'Juego de 4 toallas de algodón', 3, 39.99, 15.00, 4, 5, 80, 1),
    ('HOGR-003', 'Organizador de Escritorio', 'Organizador multiusos de bambú', 3, 25.99, 10.00, 60, 10, 150, 1),

    -- Deportes (CategoryId = 4)
    ('DEPO-001', 'Pelota de Fútbol', 'Balón oficial tamaño 5', 4, 35.99, 15.00, 20, 5, 100, 1),
    ('DEPO-002', 'Banda Elástica Set', 'Set de 5 bandas de resistencia', 4, 24.99, 8.00, 0, 5, 200, 1),
    ('DEPO-003', 'Botella Deportiva', 'Botella térmica 750ml', 4, 19.99, 7.00, 80, 15, 300, 1),

    -- Libros (CategoryId = 5)
    ('LIBR-001', 'Clean Code', 'Robert C. Martin - Código Limpio', 5, 45.99, 22.00, 12, 3, 50, 1),
    ('LIBR-002', 'Design Patterns', 'Gang of Four - Patrones de Diseño', 5, 55.99, 28.00, 8, 3, 40, 1);

-- ============================================
-- MOVIMIENTOS DE STOCK INICIALES
-- MovementTypeId = 1 (IN)
-- ============================================
INSERT INTO StockMovement (ProductId, MovementTypeId, Quantity, PreviousStock, NewStock, Reason, Reference, UserId)
SELECT
    Id,
    1, -- MovementType: IN
    Quantity,
    0,
    Quantity,
    'Carga inicial de inventario',
    'INIT-001',
    1
FROM Product
WHERE Quantity > 0;

-- ============================================
-- NOTIFICACIONES DE EJEMPLO
-- NotificationTypeId: 1 = SYSTEM, 2 = LOW_STOCK, 3 = OUT_OF_STOCK
-- ============================================

-- Productos con stock bajo
INSERT INTO Notification (UserId, ProductId, NotificationTypeId, Title, Message, IsRead)
SELECT
    1,
    p.Id,
    2, -- NotificationType: LOW_STOCK
    'Stock bajo: ' + p.Name,
    'El producto ' + p.SKU + ' - ' + p.Name + ' tiene solo ' + CAST(p.Quantity AS NVARCHAR(10)) + ' unidades. Mínimo requerido: ' + CAST(p.MinStock AS NVARCHAR(10)),
    0
FROM Product p
WHERE p.Quantity < p.MinStock AND p.Quantity > 0;

-- Productos sin stock
INSERT INTO Notification (UserId, ProductId, NotificationTypeId, Title, Message, IsRead)
SELECT
    1,
    p.Id,
    3, -- NotificationType: OUT_OF_STOCK
    'Sin stock: ' + p.Name,
    'El producto ' + p.SKU + ' - ' + p.Name + ' está agotado.',
    0
FROM Product p
WHERE p.Quantity = 0;
