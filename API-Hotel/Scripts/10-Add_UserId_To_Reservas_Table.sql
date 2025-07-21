-- =============================================
-- Script: 10-Add_UserId_To_Reservas_Table.sql
-- Description: Agrega el campo UserId a la tabla Reservas para relacionar 
--              con AspNetUsers y crear las foreign keys correspondientes
-- Author: Claude Code Assistant
-- Date: 2025-07-19
-- Version: 1.0
-- =============================================

USE [Hotel]
GO

-- Verificar si la tabla Reservas existe
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reservas')
BEGIN
    PRINT 'Iniciando actualización de la tabla Reservas...'
    
    -- Hacer backup de los datos existentes
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reservas_Backup_20250719')
    BEGIN
        SELECT * INTO Reservas_Backup_20250719 FROM Reservas
        PRINT 'Backup de datos existentes creado: Reservas_Backup_20250719'
    END
    
    -- Agregar campo UserId si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reservas' AND COLUMN_NAME = 'UserId')
    BEGIN
        ALTER TABLE Reservas ADD UserId NVARCHAR(450) NULL
        PRINT 'Columna UserId agregada a tabla Reservas (referencia a AspNetUsers)'
    END
    ELSE
    BEGIN
        PRINT 'Columna UserId ya existe en tabla Reservas'
        
        -- Verificar si tiene el tipo y tamaño correcto
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Reservas' AND COLUMN_NAME = 'UserId' 
                   AND (CHARACTER_MAXIMUM_LENGTH != 450 OR DATA_TYPE != 'nvarchar'))
        BEGIN
            ALTER TABLE Reservas ALTER COLUMN UserId NVARCHAR(450) NULL
            PRINT 'Columna UserId actualizada a NVARCHAR(450) NULL'
        END
    END
    
    -- Crear índice en UserId para mejorar performance
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reservas') AND name = 'IX_Reservas_UserId')
    BEGIN
        CREATE INDEX IX_Reservas_UserId ON Reservas(UserId)
        PRINT 'Índice IX_Reservas_UserId creado'
    END
    
    -- Crear índice compuesto en InstitucionID y UserId para consultas multi-tenant
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reservas') AND name = 'IX_Reservas_InstitucionID_UserId')
    BEGIN
        CREATE INDEX IX_Reservas_InstitucionID_UserId ON Reservas(InstitucionID, UserId)
        PRINT 'Índice IX_Reservas_InstitucionID_UserId creado'
    END
    
    -- Crear índice compuesto para consultas de reservas activas por usuario
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reservas') AND name = 'IX_Reservas_UserId_FechaFin')
    BEGIN
        CREATE INDEX IX_Reservas_UserId_FechaFin ON Reservas(UserId, FechaFin)
        PRINT 'Índice IX_Reservas_UserId_FechaFin creado para consultas de reservas activas'
    END
    
    -- Crear índice para consultas por fechas y usuario
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reservas') AND name = 'IX_Reservas_UserId_FechaReserva')
    BEGIN
        CREATE INDEX IX_Reservas_UserId_FechaReserva ON Reservas(UserId, FechaReserva DESC)
        PRINT 'Índice IX_Reservas_UserId_FechaReserva creado para consultas por fecha'
    END
    
    -- Crear foreign key con AspNetUsers si la tabla existe
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
       AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Reservas_AspNetUsers')
    BEGIN
        ALTER TABLE Reservas 
        ADD CONSTRAINT FK_Reservas_AspNetUsers 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
        PRINT 'Foreign Key FK_Reservas_AspNetUsers creada'
    END
    ELSE IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Reservas_AspNetUsers')
    BEGIN
        PRINT 'Foreign Key FK_Reservas_AspNetUsers ya existe'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Tabla AspNetUsers no encontrada. Foreign Key no creada.'
    END
    
    -- Verificar si existe el campo UsuarioId legacy para migración de datos
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reservas' AND COLUMN_NAME = 'UsuarioId')
    BEGIN
        PRINT 'Campo legacy UsuarioId encontrado. Considere migrar datos a UserId.'
        
        -- Mostrar estadísticas de datos existentes
        DECLARE @CountWithLegacyUserId INT
        DECLARE @CountWithNewUserId INT
        DECLARE @CountActiveReservations INT
        
        SELECT @CountWithLegacyUserId = COUNT(*) FROM Reservas WHERE UsuarioId IS NOT NULL
        SELECT @CountWithNewUserId = COUNT(*) FROM Reservas WHERE UserId IS NOT NULL
        SELECT @CountActiveReservations = COUNT(*) FROM Reservas WHERE FechaFin IS NULL
        
        PRINT 'Registros con UsuarioId (legacy): ' + CAST(@CountWithLegacyUserId AS VARCHAR(10))
        PRINT 'Registros con UserId (new): ' + CAST(@CountWithNewUserId AS VARCHAR(10))
        PRINT 'Reservas activas: ' + CAST(@CountActiveReservations AS VARCHAR(10))
        
        -- Ejemplo de migración de datos (comentado por seguridad)
        /*
        -- SOLO EJECUTAR DESPUÉS DE VERIFICAR LA LÓGICA DE MIGRACIÓN
        UPDATE Reservas 
        SET UserId = (SELECT Id FROM AspNetUsers WHERE LegacyUserId = Reservas.UsuarioId)
        WHERE UserId IS NULL AND UsuarioId IS NOT NULL
        AND EXISTS (SELECT 1 FROM AspNetUsers WHERE LegacyUserId = Reservas.UsuarioId)
        */
        
        PRINT 'NOTA: Para migrar datos de UsuarioId a UserId, ejecute script de migración específico'
    END
    
    -- Verificar integridad de los datos
    DECLARE @OrphanedRecords INT
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    BEGIN
        SELECT @OrphanedRecords = COUNT(*) 
        FROM Reservas 
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
    
    -- Verificar consistencia entre Reservas y Visitas
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Visitas')
       AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Visitas' AND COLUMN_NAME = 'UserId')
    BEGIN
        DECLARE @InconsistentReservations INT
        SELECT @InconsistentReservations = COUNT(*)
        FROM Reservas r
        INNER JOIN Visitas v ON r.VisitaId = v.VisitaId
        WHERE r.UserId IS NOT NULL 
        AND v.UserId IS NOT NULL 
        AND r.UserId != v.UserId
        
        IF @InconsistentReservations > 0
        BEGIN
            PRINT 'WARNING: ' + CAST(@InconsistentReservations AS VARCHAR(10)) + ' reservas con UserId diferente al de su visita asociada'
        END
        ELSE
        BEGIN
            PRINT 'Consistencia verificada: Los UserId entre Reservas y Visitas son coherentes'
        END
    END
    
    PRINT 'Tabla Reservas actualizada exitosamente!'
    PRINT ''
    PRINT 'Cambios realizados:'
    PRINT '- Campo UserId: NVARCHAR(450) NULL (referencia a AspNetUsers.Id)'
    PRINT '- Índice IX_Reservas_UserId: Para mejorar consultas por usuario'
    PRINT '- Índice IX_Reservas_InstitucionID_UserId: Para consultas multi-tenant por usuario'
    PRINT '- Índice IX_Reservas_UserId_FechaFin: Para consultas de reservas activas'
    PRINT '- Índice IX_Reservas_UserId_FechaReserva: Para consultas por fecha'
    PRINT '- Foreign Key FK_Reservas_AspNetUsers: Integridad referencial con AspNetUsers'
    PRINT ''
    PRINT 'Próximos pasos:'
    PRINT '1. Migrar datos del campo UsuarioId legacy al nuevo UserId'
    PRINT '2. Actualizar aplicación para usar el nuevo campo UserId'
    PRINT '3. Verificar consistencia de UserId entre Reservas y Visitas relacionadas'
    PRINT '4. Deprecar el campo UsuarioId legacy gradualmente'
    
END
ELSE
BEGIN
    PRINT 'Error: La tabla Reservas no existe en la base de datos'
END

GO