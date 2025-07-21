-- =============================================
-- Script: 9-Add_UserId_To_Visitas_Table.sql
-- Description: Agrega el campo UserId a la tabla Visitas para relacionar 
--              con AspNetUsers y crear las foreign keys correspondientes
-- Author: Claude Code Assistant
-- Date: 2025-07-19
-- Version: 1.0
-- =============================================

USE [Hotel]
GO

-- Verificar si la tabla Visitas existe
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Visitas')
BEGIN
    PRINT 'Iniciando actualización de la tabla Visitas...'
    
    -- Hacer backup de los datos existentes
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Visitas_Backup_20250719')
    BEGIN
        SELECT * INTO Visitas_Backup_20250719 FROM Visitas
        PRINT 'Backup de datos existentes creado: Visitas_Backup_20250719'
    END
    
    -- Agregar campo UserId si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Visitas' AND COLUMN_NAME = 'UserId')
    BEGIN
        ALTER TABLE Visitas ADD UserId NVARCHAR(450) NULL
        PRINT 'Columna UserId agregada a tabla Visitas (referencia a AspNetUsers)'
    END
    ELSE
    BEGIN
        PRINT 'Columna UserId ya existe en tabla Visitas'
        
        -- Verificar si tiene el tipo y tamaño correcto
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Visitas' AND COLUMN_NAME = 'UserId' 
                   AND (CHARACTER_MAXIMUM_LENGTH != 450 OR DATA_TYPE != 'nvarchar'))
        BEGIN
            ALTER TABLE Visitas ALTER COLUMN UserId NVARCHAR(450) NULL
            PRINT 'Columna UserId actualizada a NVARCHAR(450) NULL'
        END
    END
    
    -- Crear índice en UserId para mejorar performance
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Visitas') AND name = 'IX_Visitas_UserId')
    BEGIN
        CREATE INDEX IX_Visitas_UserId ON Visitas(UserId)
        PRINT 'Índice IX_Visitas_UserId creado'
    END
    
    -- Crear índice compuesto en InstitucionID y UserId para consultas multi-tenant
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Visitas') AND name = 'IX_Visitas_InstitucionID_UserId')
    BEGIN
        CREATE INDEX IX_Visitas_InstitucionID_UserId ON Visitas(InstitucionID, UserId)
        PRINT 'Índice IX_Visitas_InstitucionID_UserId creado'
    END
    
    -- Crear foreign key con AspNetUsers si la tabla existe
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
       AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Visitas_AspNetUsers')
    BEGIN
        ALTER TABLE Visitas 
        ADD CONSTRAINT FK_Visitas_AspNetUsers 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
        PRINT 'Foreign Key FK_Visitas_AspNetUsers creada'
    END
    ELSE IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Visitas_AspNetUsers')
    BEGIN
        PRINT 'Foreign Key FK_Visitas_AspNetUsers ya existe'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Tabla AspNetUsers no encontrada. Foreign Key no creada.'
    END
    
    -- Verificar si existe el campo UsuarioId legacy para migración de datos
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Visitas' AND COLUMN_NAME = 'UsuarioId')
    BEGIN
        PRINT 'Campo legacy UsuarioId encontrado. Considere migrar datos a UserId.'
        
        -- Mostrar estadísticas de datos existentes
        DECLARE @CountWithLegacyUserId INT
        DECLARE @CountWithNewUserId INT
        
        SELECT @CountWithLegacyUserId = COUNT(*) FROM Visitas WHERE UsuarioId IS NOT NULL
        SELECT @CountWithNewUserId = COUNT(*) FROM Visitas WHERE UserId IS NOT NULL
        
        PRINT 'Registros con UsuarioId (legacy): ' + CAST(@CountWithLegacyUserId AS VARCHAR(10))
        PRINT 'Registros con UserId (new): ' + CAST(@CountWithNewUserId AS VARCHAR(10))
        
        -- Ejemplo de migración de datos (comentado por seguridad)
        /*
        -- SOLO EJECUTAR DESPUÉS DE VERIFICAR LA LÓGICA DE MIGRACIÓN
        UPDATE Visitas 
        SET UserId = (SELECT Id FROM AspNetUsers WHERE LegacyUserId = Visitas.UsuarioId)
        WHERE UserId IS NULL AND UsuarioId IS NOT NULL
        AND EXISTS (SELECT 1 FROM AspNetUsers WHERE LegacyUserId = Visitas.UsuarioId)
        */
        
        PRINT 'NOTA: Para migrar datos de UsuarioId a UserId, ejecute script de migración específico'
    END
    
    -- Verificar integridad de los datos
    DECLARE @OrphanedRecords INT
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    BEGIN
        SELECT @OrphanedRecords = COUNT(*) 
        FROM Visitas 
        WHERE UserId IS NOT NULL 
        AND UserId NOT IN (SELECT Id FROM AspNetUsers)
        
        IF @OrphanedRecords > 0
        BEGIN
            PRINT 'WARNING: ' + CAST(@OrphanedRecords AS VARCHAR(10)) + ' registros con UserId que no existen en AspNetUsers'
        END
        ELSE
        BEGIN
            PRINT 'Integridad de datos verificada: Todos los UserId tienen registros válidos en AspNetUsers'
        END
    END
    
    PRINT 'Tabla Visitas actualizada exitosamente!'
    PRINT ''
    PRINT 'Cambios realizados:'
    PRINT '- Campo UserId: NVARCHAR(450) NULL (referencia a AspNetUsers.Id)'
    PRINT '- Índice IX_Visitas_UserId: Para mejorar consultas por usuario'
    PRINT '- Índice IX_Visitas_InstitucionID_UserId: Para consultas multi-tenant por usuario'
    PRINT '- Foreign Key FK_Visitas_AspNetUsers: Integridad referencial con AspNetUsers'
    PRINT ''
    PRINT 'Próximos pasos:'
    PRINT '1. Migrar datos del campo UsuarioId legacy al nuevo UserId'
    PRINT '2. Actualizar aplicación para usar el nuevo campo UserId'
    PRINT '3. Deprecar el campo UsuarioId legacy gradualmente'
    
END
ELSE
BEGIN
    PRINT 'Error: La tabla Visitas no existe en la base de datos'
END

GO