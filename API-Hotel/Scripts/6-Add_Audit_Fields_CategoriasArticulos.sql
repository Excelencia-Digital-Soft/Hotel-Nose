-- =============================================
-- Script: Add Audit Fields and Image Support to CategoriasArticulos Table
-- Description: Adds audit fields and image support for tracking creation and modification with AspNetUsers
-- Version: 1.0
-- Date: 2024-07-15
-- =============================================

-- Verificar si existe la tabla CategoriasArticulos
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CategoriasArticulos')
BEGIN
    PRINT 'Adding audit fields and image support to CategoriasArticulos table...'
    
    -- Agregar campo de imagen si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoriasArticulos' AND COLUMN_NAME = 'imagenID')
    BEGIN
        ALTER TABLE CategoriasArticulos ADD imagenID INT NULL
        PRINT 'Added imagenID column'
    END
    
    -- Agregar campos de auditoría si no existen
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoriasArticulos' AND COLUMN_NAME = 'CreadoPorId')
    BEGIN
        ALTER TABLE CategoriasArticulos ADD CreadoPorId NVARCHAR(450) NULL
        PRINT 'Added CreadoPorId column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoriasArticulos' AND COLUMN_NAME = 'FechaCreacion')
    BEGIN
        ALTER TABLE CategoriasArticulos ADD FechaCreacion DATETIME2 NULL
        PRINT 'Added FechaCreacion column'
        
        -- Migrar datos existentes: Agregar fecha actual donde FechaCreacion sea NULL
        EXEC('UPDATE CategoriasArticulos SET FechaCreacion = GETDATE() WHERE FechaCreacion IS NULL')
        
        PRINT 'Migrated existing records with current date for FechaCreacion'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoriasArticulos' AND COLUMN_NAME = 'FechaRegistro')
    BEGIN
        ALTER TABLE CategoriasArticulos ADD FechaRegistro DATETIME2 NULL
        PRINT 'Added FechaRegistro column'
        
        -- Migrar datos existentes: Copiar FechaCreacion a FechaRegistro donde FechaRegistro sea NULL
        EXEC('UPDATE CategoriasArticulos SET FechaRegistro = FechaCreacion WHERE FechaRegistro IS NULL AND FechaCreacion IS NOT NULL')
        
        PRINT 'Migrated existing FechaCreacion to FechaRegistro'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoriasArticulos' AND COLUMN_NAME = 'ModificadoPorId')
    BEGIN
        ALTER TABLE CategoriasArticulos ADD ModificadoPorId NVARCHAR(450) NULL
        PRINT 'Added ModificadoPorId column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoriasArticulos' AND COLUMN_NAME = 'FechaModificacion')
    BEGIN
        ALTER TABLE CategoriasArticulos ADD FechaModificacion DATETIME2 NULL
        PRINT 'Added FechaModificacion column'
    END
    
    -- Crear índices para mejorar performance
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CategoriasArticulos_CreadoPorId')
    BEGIN
        CREATE INDEX IX_CategoriasArticulos_CreadoPorId ON CategoriasArticulos (CreadoPorId)
        PRINT 'Created index IX_CategoriasArticulos_CreadoPorId'
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CategoriasArticulos_ModificadoPorId')
    BEGIN
        CREATE INDEX IX_CategoriasArticulos_ModificadoPorId ON CategoriasArticulos (ModificadoPorId)
        PRINT 'Created index IX_CategoriasArticulos_ModificadoPorId'
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CategoriasArticulos_imagenID')
    BEGIN
        CREATE INDEX IX_CategoriasArticulos_imagenID ON CategoriasArticulos (imagenID)
        PRINT 'Created index IX_CategoriasArticulos_imagenID'
    END
    
    -- Crear foreign keys si existen las tablas relacionadas
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    BEGIN
        -- FK para CreadoPorId
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_CategoriasArticulos_AspNetUsers_CreadoPorId')
        BEGIN
            ALTER TABLE CategoriasArticulos 
            ADD CONSTRAINT FK_CategoriasArticulos_AspNetUsers_CreadoPorId 
            FOREIGN KEY (CreadoPorId) REFERENCES AspNetUsers(Id)
            PRINT 'Created FK_CategoriasArticulos_AspNetUsers_CreadoPorId'
        END
        
        -- FK para ModificadoPorId
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_CategoriasArticulos_AspNetUsers_ModificadoPorId')
        BEGIN
            ALTER TABLE CategoriasArticulos 
            ADD CONSTRAINT FK_CategoriasArticulos_AspNetUsers_ModificadoPorId 
            FOREIGN KEY (ModificadoPorId) REFERENCES AspNetUsers(Id)
            PRINT 'Created FK_CategoriasArticulos_AspNetUsers_ModificadoPorId'
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: AspNetUsers table not found. Foreign keys for audit fields not created.'
    END
    
    -- FK para imagenID
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Imagenes')
    BEGIN
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_CategoriasArticulos_Imagenes_imagenID')
        BEGIN
            ALTER TABLE CategoriasArticulos 
            ADD CONSTRAINT FK_CategoriasArticulos_Imagenes_imagenID 
            FOREIGN KEY (imagenID) REFERENCES Imagenes(ImagenId)
            PRINT 'Created FK_CategoriasArticulos_Imagenes_imagenID'
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Imagenes table not found. Foreign key for imagenID not created.'
    END
    
    PRINT 'CategoriasArticulos table audit fields and image support migration completed successfully!'
END
ELSE
BEGIN
    PRINT 'ERROR: CategoriasArticulos table not found!'
END

PRINT '============================================='
PRINT 'Migration script completed!'
PRINT 'Please verify the changes and update any default values as needed.'
PRINT '============================================='