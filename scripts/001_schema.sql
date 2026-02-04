-- ============================================
-- Sistema de Gestión de Inventarios
-- Schema: 001_schema.sql
-- Creado por: Jose Murga
-- ============================================

-- ============================================
-- CATÁLOGOS
-- ============================================

-- Tipos de documento de identidad
CREATE TABLE DocumentType (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Code            NVARCHAR(10) NOT NULL UNIQUE,  -- 'DNI', 'RUC', 'CE', 'PASSPORT'
    Name            NVARCHAR(50) NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1
);

-- Tipos de movimiento de inventario
CREATE TABLE MovementType (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Code            NVARCHAR(20) NOT NULL UNIQUE,  -- 'IN', 'OUT', 'ADJUSTMENT'
    Name            NVARCHAR(50) NOT NULL,
    Description     NVARCHAR(255) NULL,
    IsActive        BIT NOT NULL DEFAULT 1
);

-- Tipos de notificación
CREATE TABLE NotificationType (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Code            NVARCHAR(30) NOT NULL UNIQUE,  -- 'LOW_STOCK', 'OUT_OF_STOCK', 'SYSTEM'
    Name            NVARCHAR(50) NOT NULL,
    Description     NVARCHAR(255) NULL,
    IsActive        BIT NOT NULL DEFAULT 1
);

-- Acciones de auditoría
CREATE TABLE AuditAction (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Code            NVARCHAR(20) NOT NULL UNIQUE,  -- 'CREATED', 'MODIFIED', 'DELETED'
    Name            NVARCHAR(50) NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1
);

-- ============================================
-- TABLA: Role
-- Roles del sistema (Admin, Empleado)
-- ============================================
CREATE TABLE Role (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(50) NOT NULL UNIQUE,
    Description     NVARCHAR(255) NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================
-- TABLA: Person
-- Datos personales (separado de autenticación)
-- ============================================
CREATE TABLE Person (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FirstName       NVARCHAR(100) NOT NULL,
    LastName        NVARCHAR(100) NOT NULL,
    DocumentTypeId  INT NULL,
    DocumentNumber  NVARCHAR(20) NULL,
    Phone           NVARCHAR(20) NULL,
    Address         NVARCHAR(500) NULL,
    BirthDate       DATE NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NULL,

    CONSTRAINT FK_Person_DocumentType FOREIGN KEY (DocumentTypeId)
        REFERENCES DocumentType(Id),
    CONSTRAINT UQ_Person_Document UNIQUE (DocumentTypeId, DocumentNumber)
);

-- ============================================
-- TABLA: User
-- Credenciales de autenticación
-- ============================================
CREATE TABLE [User] (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PersonId        INT NOT NULL,
    Email           NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(500) NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt     DATETIME2 NULL,

    CONSTRAINT FK_User_Person FOREIGN KEY (PersonId)
        REFERENCES Person(Id)
);

-- ============================================
-- TABLA: UserRole
-- Relación muchos a muchos: User <-> Role
-- ============================================
CREATE TABLE UserRole (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    RoleId          INT NOT NULL,
    AssignedAt      DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy      INT NULL,

    CONSTRAINT FK_UserRole_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId)
        REFERENCES Role(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRole_AssignedBy FOREIGN KEY (AssignedBy)
        REFERENCES [User](Id),
    CONSTRAINT UQ_UserRole_UserRole UNIQUE (UserId, RoleId)
);

-- ============================================
-- TABLA: Category
-- Categorías de productos
-- ============================================
CREATE TABLE Category (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100) NOT NULL UNIQUE,
    Description     NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================
-- TABLA: Product
-- Productos del inventario
-- ============================================
CREATE TABLE Product (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    SKU             NVARCHAR(50) NOT NULL UNIQUE,
    Name            NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(1000) NULL,
    CategoryId      INT NOT NULL,
    UnitPrice       DECIMAL(18,2) NOT NULL,
    Cost            DECIMAL(18,2) NULL,
    Quantity        INT NOT NULL DEFAULT 0,
    MinStock        INT NOT NULL DEFAULT 5,
    MaxStock        INT NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NULL,

    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId)
        REFERENCES Category(Id),
    CONSTRAINT CK_Product_Quantity_NonNegative CHECK (Quantity >= 0),
    CONSTRAINT CK_Product_UnitPrice_Positive CHECK (UnitPrice >= 0),
    CONSTRAINT CK_Product_Cost_Positive CHECK (Cost IS NULL OR Cost >= 0),
    CONSTRAINT CK_Product_MinStock_NonNegative CHECK (MinStock >= 0)
);

-- ============================================
-- TABLA: StockMovement
-- Historial de movimientos de inventario
-- ============================================
CREATE TABLE StockMovement (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL,
    MovementTypeId  INT NOT NULL,
    Quantity        INT NOT NULL,
    PreviousStock   INT NOT NULL,
    NewStock        INT NOT NULL,
    Reason          NVARCHAR(500) NULL,
    Reference       NVARCHAR(100) NULL,
    UserId          INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_StockMovement_Product FOREIGN KEY (ProductId)
        REFERENCES Product(Id),
    CONSTRAINT FK_StockMovement_MovementType FOREIGN KEY (MovementTypeId)
        REFERENCES MovementType(Id),
    CONSTRAINT FK_StockMovement_User FOREIGN KEY (UserId)
        REFERENCES [User](Id)
);

-- ============================================
-- TABLA: Notification
-- Notificaciones del sistema
-- ============================================
CREATE TABLE Notification (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    ProductId       INT NULL,
    NotificationTypeId INT NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Message         NVARCHAR(1000) NOT NULL,
    IsRead          BIT NOT NULL DEFAULT 0,
    ReadAt          DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE CASCADE,
    CONSTRAINT FK_Notification_Product FOREIGN KEY (ProductId)
        REFERENCES Product(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Notification_NotificationType FOREIGN KEY (NotificationTypeId)
        REFERENCES NotificationType(Id)
);

-- ============================================
-- TABLA: AuditLog
-- Registro de auditoría del sistema
-- ============================================
CREATE TABLE AuditLog (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName       NVARCHAR(100) NOT NULL,
    RecordId        INT NOT NULL,
    AuditActionId   INT NOT NULL,
    OldValues       NVARCHAR(MAX) NULL,
    NewValues       NVARCHAR(MAX) NULL,
    UserId          INT NULL,
    IpAddress       NVARCHAR(45) NULL,
    UserAgent       NVARCHAR(500) NULL,
    Message         NVARCHAR(500) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_AuditLog_AuditAction FOREIGN KEY (AuditActionId)
        REFERENCES AuditAction(Id),
    CONSTRAINT FK_AuditLog_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE SET NULL
);

-- ============================================
-- ÍNDICES
-- ============================================

-- Person
CREATE INDEX IX_Person_DocumentNumber ON Person(DocumentNumber);
CREATE INDEX IX_Person_FullName ON Person(LastName, FirstName);

-- User
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_PersonId ON [User](PersonId);
CREATE INDEX IX_User_IsActive ON [User](IsActive);

-- Product
CREATE INDEX IX_Product_SKU ON Product(SKU);
CREATE INDEX IX_Product_CategoryId ON Product(CategoryId);
CREATE INDEX IX_Product_IsActive ON Product(IsActive);
CREATE INDEX IX_Product_Quantity ON Product(Quantity);

-- StockMovement
CREATE INDEX IX_StockMovement_ProductId ON StockMovement(ProductId);
CREATE INDEX IX_StockMovement_CreatedAt ON StockMovement(CreatedAt);
CREATE INDEX IX_StockMovement_UserId ON StockMovement(UserId);
CREATE INDEX IX_StockMovement_MovementTypeId ON StockMovement(MovementTypeId);

-- Notification
CREATE INDEX IX_Notification_UserId ON Notification(UserId);
CREATE INDEX IX_Notification_IsRead ON Notification(IsRead);
CREATE INDEX IX_Notification_CreatedAt ON Notification(CreatedAt);
CREATE INDEX IX_Notification_NotificationTypeId ON Notification(NotificationTypeId);

-- AuditLog
CREATE INDEX IX_AuditLog_TableName ON AuditLog(TableName);
CREATE INDEX IX_AuditLog_RecordId ON AuditLog(RecordId);
CREATE INDEX IX_AuditLog_CreatedAt ON AuditLog(CreatedAt);
CREATE INDEX IX_AuditLog_UserId ON AuditLog(UserId);
CREATE INDEX IX_AuditLog_AuditActionId ON AuditLog(AuditActionId);
