-- =============================================
-- Script: 8-Improve_Registros_Table_Structure.sql
-- Description: Mejora la estructura de la tabla Registros con campos de auditoría 
--              y información más relevante para el sistema
-- Author: Claude Code Assistant
-- Date: 2025-07-19
-- Version: 1.0
-- =============================================

USE [Hotel]
GO

-- Verificar si la tabla Registros existe
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Registros')
BEGIN
    PRINT 'Iniciando mejoras en la tabla Registros...'
    
    -- Hacer backup de los datos existentes
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Registros_Backup_20250719')
    BEGIN
        SELECT * INTO Registros_Backup_20250719 FROM Registros
        PRINT 'Backup de datos existentes creado: Registros_Backup_20250719'
    END
    
    -- Agregar nuevas columnas si no existen
    
    -- Campo TipoRegistro (enum)
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'TipoRegistro')
    BEGIN
        ALTER TABLE Registros ADD TipoRegistro INT NOT NULL DEFAULT 1
        PRINT 'Columna TipoRegistro agregada (1=INFO, 2=WARNING, 3=ERROR, 4=AUDIT, 5=DEBUG, 6=SECURITY)'
    END
    
    -- Campo Modulo (enum)
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'Modulo')
    BEGIN
        ALTER TABLE Registros ADD Modulo INT NOT NULL DEFAULT 10
        PRINT 'Columna Modulo agregada (1=RESERVAS, 2=PAGOS, 3=HABITACIONES, 4=USUARIOS, 5=CONSUMOS, 6=PROMOCIONES, 7=CONFIGURACION, 8=INVENTARIO, 9=REPORTES, 10=SISTEMA)'
    END
    
    -- Campo UsuarioId (referencia a AspNetUsers)
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'UsuarioId')
    BEGIN
        ALTER TABLE Registros ADD UsuarioId NVARCHAR(450) NULL
        PRINT 'Columna UsuarioId agregada (referencia a AspNetUsers)'
    END
    
    -- Campo InstitucionID para multi-tenancy
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'InstitucionID')
    BEGIN
        ALTER TABLE Registros ADD InstitucionID INT NOT NULL DEFAULT 1
        PRINT 'Columna InstitucionID agregada para multi-tenancy'
    END
    
    -- Campo FechaRegistro con default
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'FechaRegistro')
    BEGIN
        ALTER TABLE Registros ADD FechaRegistro DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
        PRINT 'Columna FechaRegistro agregada con default UTC'
    END
    
    -- Campo DetallesAdicionales para JSON
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'DetallesAdicionales')
    BEGIN
        ALTER TABLE Registros ADD DetallesAdicionales NVARCHAR(MAX) NULL
        PRINT 'Columna DetallesAdicionales agregada para JSON'
    END
    
    -- Campo DireccionIP
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'DireccionIP')
    BEGIN
        ALTER TABLE Registros ADD DireccionIP NVARCHAR(45) NULL
        PRINT 'Columna DireccionIP agregada'
    END
    
    -- Campo Anulado para soft delete
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'Anulado')
    BEGIN
        ALTER TABLE Registros ADD Anulado BIT NULL
        PRINT 'Columna Anulado agregada para soft delete'
    END
    
    -- Modificar columnas existentes si es necesario
    
    -- Verificar y modificar Contenido si no tiene la longitud correcta
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'Contenido' 
               AND (CHARACTER_MAXIMUM_LENGTH != 2000 OR IS_NULLABLE = 'YES'))
    BEGIN
        -- Primero actualizar cualquier NULL existente
        UPDATE Registros SET Contenido = '' WHERE Contenido IS NULL
        
        -- Modificar la columna
        ALTER TABLE Registros ALTER COLUMN Contenido NVARCHAR(2000) NOT NULL
        PRINT 'Columna Contenido modificada a NVARCHAR(2000) NOT NULL'
    END
    
    -- Asegurar que ReservaId sea nullable
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'ReservaId' 
               AND IS_NULLABLE = 'NO')
    BEGIN
        ALTER TABLE Registros ALTER COLUMN ReservaId INT NULL
        PRINT 'Columna ReservaId modificada a nullable'
    END
    
    -- Crear índices para mejorar performance
    
    -- Índice en FechaRegistro para consultas por fechas
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Registros') AND name = 'IX_Registros_FechaRegistro')
    BEGIN
        CREATE INDEX IX_Registros_FechaRegistro ON Registros(FechaRegistro DESC)
        PRINT 'Índice IX_Registros_FechaRegistro creado'
    END
    
    -- Índice en InstitucionID para multi-tenancy
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Registros') AND name = 'IX_Registros_InstitucionID')
    BEGIN
        CREATE INDEX IX_Registros_InstitucionID ON Registros(InstitucionID)
        PRINT 'Índice IX_Registros_InstitucionID creado'
    END
    
    -- Índice compuesto en TipoRegistro y Modulo para filtros comunes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Registros') AND name = 'IX_Registros_TipoRegistro_Modulo')
    BEGIN
        CREATE INDEX IX_Registros_TipoRegistro_Modulo ON Registros(TipoRegistro, Modulo)
        PRINT 'Índice IX_Registros_TipoRegistro_Modulo creado'
    END
    
    -- Índice en UsuarioId para consultas por usuario
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Registros') AND name = 'IX_Registros_UsuarioId')
    BEGIN
        CREATE INDEX IX_Registros_UsuarioId ON Registros(UsuarioId)
        PRINT 'Índice IX_Registros_UsuarioId creado'
    END
    
    -- Crear foreign keys si las tablas relacionadas existen
    
    -- FK con AspNetUsers (si existe)
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
       AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Registros_AspNetUsers')
    BEGIN
        ALTER TABLE Registros 
        ADD CONSTRAINT FK_Registros_AspNetUsers 
        FOREIGN KEY (UsuarioId) REFERENCES AspNetUsers(Id)
        PRINT 'Foreign Key FK_Registros_AspNetUsers creada'
    END
    
    -- FK con Institucion (si existe)
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Institucion')
       AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Registros_Institucion')
    BEGIN
        ALTER TABLE Registros 
        ADD CONSTRAINT FK_Registros_Institucion 
        FOREIGN KEY (InstitucionID) REFERENCES Institucion(InstitucionId)
        PRINT 'Foreign Key FK_Registros_Institucion creada'
    END
    
    -- FK con Reservas (si existe y no está creada)
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reservas')
       AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Registros_Reservas')
    BEGIN
        -- Primero, limpiar referencias huérfanas (ReservaId que no existen en Reservas)
        PRINT 'Verificando referencias huérfanas de ReservaId...'
        
        DECLARE @orphanedCount INT
        SELECT @orphanedCount = COUNT(*) 
        FROM Registros r 
        WHERE r.ReservaId IS NOT NULL 
          AND NOT EXISTS (SELECT 1 FROM Reservas res WHERE res.ReservaId = r.ReservaId)
        
        IF @orphanedCount > 0
        BEGIN
            PRINT 'Encontradas ' + CAST(@orphanedCount AS VARCHAR(10)) + ' referencias huérfanas. Estableciendo a NULL...'
            
            -- Actualizar registros huérfanos a NULL
            UPDATE r
            SET ReservaId = NULL
            FROM Registros r
            WHERE r.ReservaId IS NOT NULL 
              AND NOT EXISTS (SELECT 1 FROM Reservas res WHERE res.ReservaId = r.ReservaId)
            
            PRINT 'Referencias huérfanas actualizadas a NULL'
        END
        ELSE
        BEGIN
            PRINT 'No se encontraron referencias huérfanas'
        END
        
        -- Ahora crear la foreign key
        ALTER TABLE Registros 
        ADD CONSTRAINT FK_Registros_Reservas 
        FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId)
        PRINT 'Foreign Key FK_Registros_Reservas creada'
    END
    
    -- Insertar algunos registros de ejemplo para demostrar la nueva estructura
    IF NOT EXISTS (SELECT TOP 1 * FROM Registros WHERE TipoRegistro IS NOT NULL AND Modulo IS NOT NULL)
    BEGIN
        INSERT INTO Registros (Contenido, TipoRegistro, Modulo, InstitucionID, FechaRegistro, DetallesAdicionales)
        VALUES 
        ('Sistema de registros mejorado e implementado', 4, 10, 1, GETUTCDATE(), '{"version": "1.0", "action": "system_upgrade"}'),
        ('Estructura de tabla Registros actualizada', 1, 10, 1, GETUTCDATE(), '{"script": "8-Improve_Registros_Table_Structure.sql"}')
        
        PRINT 'Registros de ejemplo insertados'
    END
    
    PRINT 'Tabla Registros mejorada exitosamente!'
    PRINT ''
    PRINT 'Nuevos campos agregados:'
    PRINT '- TipoRegistro: INT (enum para tipos de registro)'
    PRINT '- Modulo: INT (enum para módulos del sistema)'
    PRINT '- UsuarioId: NVARCHAR(450) (referencia a AspNetUsers)'
    PRINT '- InstitucionID: INT (para multi-tenancy)'
    PRINT '- FechaRegistro: DATETIME2(7) (timestamp UTC)'
    PRINT '- DetallesAdicionales: NVARCHAR(MAX) (JSON con detalles)'
    PRINT '- DireccionIP: NVARCHAR(45) (IP del usuario)'
    PRINT '- Anulado: BIT (para soft delete)'
    PRINT ''
    PRINT 'Enums definidos:'
    PRINT 'TipoRegistro: 1=INFO, 2=WARNING, 3=ERROR, 4=AUDIT, 5=DEBUG, 6=SECURITY'
    PRINT 'Modulo: 1=RESERVAS, 2=PAGOS, 3=HABITACIONES, 4=USUARIOS, 5=CONSUMOS, 6=PROMOCIONES, 7=CONFIGURACION, 8=INVENTARIO, 9=REPORTES, 10=SISTEMA'
    
END
ELSE
BEGIN
    PRINT 'Error: La tabla Registros no existe en la base de datos'
END

GO