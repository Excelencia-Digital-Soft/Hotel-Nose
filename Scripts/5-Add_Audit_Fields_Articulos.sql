-- =============================================
-- Script: Add Audit Fields to Articulos Table
-- Description: Adds audit fields for tracking creation and modification with AspNetUsers
-- Version: 1.0
-- Date: 2024-07-15
-- =============================================

-- Verificar si existe la tabla Articulos
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Articulos')
BEGIN
    PRINT 'Adding audit fields to Articulos table...'
    
    -- Agregar campos de auditoría si no existen
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Articulos' AND COLUMN_NAME = 'CreadoPorId')
    BEGIN
        ALTER TABLE Articulos ADD CreadoPorId NVARCHAR(450) NULL
        PRINT 'Added CreadoPorId column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Articulos' AND COLUMN_NAME = 'FechaCreacion')
    BEGIN
        ALTER TABLE Articulos ADD FechaCreacion DATETIME2 NULL
        PRINT 'Added FechaCreacion column'
        
        -- Migrar datos existentes usando SQL dinámico para evitar errores de parsing
        EXEC('UPDATE Articulos SET FechaCreacion = FechaRegistro WHERE FechaCreacion IS NULL AND FechaRegistro IS NOT NULL')
        
        PRINT 'Migrated existing FechaRegistro to FechaCreacion'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Articulos' AND COLUMN_NAME = 'ModificadoPorId')
    BEGIN
        ALTER TABLE Articulos ADD ModificadoPorId NVARCHAR(450) NULL
        PRINT 'Added ModificadoPorId column'
    END
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Articulos' AND COLUMN_NAME = 'FechaModificacion')
    BEGIN
        ALTER TABLE Articulos ADD FechaModificacion DATETIME2 NULL
        PRINT 'Added FechaModificacion column'
    END
    
    -- Crear índices para mejorar performance
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Articulos_CreadoPorId')
    BEGIN
        CREATE INDEX IX_Articulos_CreadoPorId ON Articulos (CreadoPorId)
        PRINT 'Created index IX_Articulos_CreadoPorId'
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Articulos_ModificadoPorId')
    BEGIN
        CREATE INDEX IX_Articulos_ModificadoPorId ON Articulos (ModificadoPorId)
        PRINT 'Created index IX_Articulos_ModificadoPorId'
    END
    
    -- Crear foreign keys a AspNetUsers si existe la tabla
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    BEGIN
        -- FK para CreadoPorId
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Articulos_AspNetUsers_CreadoPorId')
        BEGIN
            ALTER TABLE Articulos 
            ADD CONSTRAINT FK_Articulos_AspNetUsers_CreadoPorId 
            FOREIGN KEY (CreadoPorId) REFERENCES AspNetUsers(Id)
            PRINT 'Created FK_Articulos_AspNetUsers_CreadoPorId'
        END
        
        -- FK para ModificadoPorId
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Articulos_AspNetUsers_ModificadoPorId')
        BEGIN
            ALTER TABLE Articulos 
            ADD CONSTRAINT FK_Articulos_AspNetUsers_ModificadoPorId 
            FOREIGN KEY (ModificadoPorId) REFERENCES AspNetUsers(Id)
            PRINT 'Created FK_Articulos_AspNetUsers_ModificadoPorId'
        END
    END
    ELSE
    BEGIN
        PRINT 'WARNING: AspNetUsers table not found. Foreign keys not created.'
    END
    
    PRINT 'Articulos table audit fields migration completed successfully!'
END
ELSE
BEGIN
    PRINT 'ERROR: Articulos table not found!'
END

-- Verificar también la tabla Imagenes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Imagenes')
BEGIN
    PRINT 'Checking Imagenes table...'
    
    -- Agregar campo Origen si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'Origen')
    BEGIN
        ALTER TABLE Imagenes ADD Origen NVARCHAR(50) NULL DEFAULT 'Legacy'
        PRINT 'Added Origen column to Imagenes table'
        
        -- Actualizar registros existentes usando SQL dinámico
        EXEC('UPDATE Imagenes SET Origen = ''Legacy'' WHERE Origen IS NULL')
        PRINT 'Updated existing Imagenes records with Origen = Legacy'
    END
    
    -- Verificar si FechaSubida existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'FechaSubida')
    BEGIN
        ALTER TABLE Imagenes ADD FechaSubida DATETIME2 NULL DEFAULT GETDATE()
        PRINT 'Added FechaSubida column to Imagenes table'
        
        -- Actualizar registros existentes usando SQL dinámico
        EXEC('UPDATE Imagenes SET FechaSubida = GETDATE() WHERE FechaSubida IS NULL')
        PRINT 'Updated existing Imagenes records with current date'
    END
    
    -- Verificar si InstitucionID existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'InstitucionID')
    BEGIN
        ALTER TABLE Imagenes ADD InstitucionID INT NOT NULL DEFAULT 1
        PRINT 'Added InstitucionID column to Imagenes table'
        
        PRINT 'WARNING: InstitucionID set to default value 1. You may need to update this manually.'
    END
    
    PRINT 'Imagenes table verification completed!'
END
ELSE
BEGIN
    PRINT 'WARNING: Imagenes table not found!'
END

PRINT '============================================='
PRINT 'Migration script completed!'
PRINT 'Please verify the changes and update any default values as needed.'
PRINT '============================================='