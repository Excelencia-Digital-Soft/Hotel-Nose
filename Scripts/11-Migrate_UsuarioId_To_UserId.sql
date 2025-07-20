-- =============================================
-- Script: 11-Migrate_UsuarioId_To_UserId.sql
-- Description: Migra datos del campo legacy UsuarioId al nuevo UserId
--              en las tablas Visitas y Reservas
-- Author: Claude Code Assistant
-- Date: 2025-07-19
-- Version: 1.0
-- IMPORTANTE: Este script debe ejecutarse DESPUÉS de crear un mapeo
--             entre los IDs legacy y los nuevos AspNetUsers.Id
-- =============================================

USE [HotelDB]
GO

-- Variables para control del proceso
DECLARE @MigrationDate DATETIME = GETUTCDATE()
DECLARE @LogTable NVARCHAR(128) = 'UsuarioId_Migration_Log_' + FORMAT(@MigrationDate, 'yyyyMMdd_HHmmss')

PRINT 'Iniciando migración de UsuarioId legacy a UserId...'
PRINT 'Fecha de migración: ' + CAST(@MigrationDate AS VARCHAR(50))

-- Crear tabla de log para rastrear la migración
DECLARE @CreateLogTableSQL NVARCHAR(MAX) = 
    'CREATE TABLE ' + @LogTable + ' (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        TableName NVARCHAR(50),
        RecordId INT,
        LegacyUsuarioId INT,
        NewUserId NVARCHAR(450),
        MigrationStatus NVARCHAR(20),
        ErrorMessage NVARCHAR(MAX),
        MigrationDate DATETIME DEFAULT GETUTCDATE()
    )'

EXEC sp_executesql @CreateLogTableSQL
PRINT 'Tabla de log creada: ' + @LogTable

-- =============================================
-- PASO 1: Verificar pre-requisitos
-- =============================================

-- Verificar que existe tabla de mapeo (debe ser creada previamente)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LegacyUser_AspNetUser_Mapping')
BEGIN
    PRINT 'ERROR: Debe crear primero la tabla LegacyUser_AspNetUser_Mapping con el mapeo:'
    PRINT 'CREATE TABLE LegacyUser_AspNetUser_Mapping ('
    PRINT '    LegacyUsuarioId INT PRIMARY KEY,'
    PRINT '    AspNetUserId NVARCHAR(450) NOT NULL,'
    PRINT '    UserName NVARCHAR(256),'
    PRINT '    Email NVARCHAR(256),'
    PRINT '    MappingDate DATETIME DEFAULT GETUTCDATE()'
    PRINT ')'
    PRINT ''
    PRINT 'Ejemplo de datos en la tabla de mapeo:'
    PRINT 'INSERT INTO LegacyUser_AspNetUser_Mapping (LegacyUsuarioId, AspNetUserId, UserName, Email)'
    PRINT 'VALUES (1, ''user-guid-from-aspnetusers'', ''username'', ''email@domain.com'')'
    RETURN
END

-- Verificar que las tablas objetivo existen
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Visitas')
BEGIN
    PRINT 'ERROR: Tabla Visitas no encontrada'
    RETURN
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reservas')
BEGIN
    PRINT 'ERROR: Tabla Reservas no encontrada'
    RETURN
END

-- Verificar que los campos UserId existen
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Visitas' AND COLUMN_NAME = 'UserId')
BEGIN
    PRINT 'ERROR: Campo UserId no encontrado en tabla Visitas. Ejecute primero el script 9-Add_UserId_To_Visitas_Table.sql'
    RETURN
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reservas' AND COLUMN_NAME = 'UserId')
BEGIN
    PRINT 'ERROR: Campo UserId no encontrado en tabla Reservas. Ejecute primero el script 10-Add_UserId_To_Reservas_Table.sql'
    RETURN
END

-- =============================================
-- PASO 2: Mostrar estadísticas pre-migración
-- =============================================

DECLARE @VisitasConLegacyId INT, @VisitasConNewId INT, @VisitasSinId INT
DECLARE @ReservasConLegacyId INT, @ReservasConNewId INT, @ReservasSinId INT
DECLARE @MappingRecords INT

SELECT @MappingRecords = COUNT(*) FROM LegacyUser_AspNetUser_Mapping

SELECT @VisitasConLegacyId = COUNT(*) FROM Visitas WHERE UsuarioId IS NOT NULL
SELECT @VisitasConNewId = COUNT(*) FROM Visitas WHERE UserId IS NOT NULL
SELECT @VisitasSinId = COUNT(*) FROM Visitas WHERE UsuarioId IS NULL AND UserId IS NULL

SELECT @ReservasConLegacyId = COUNT(*) FROM Reservas WHERE UsuarioId IS NOT NULL
SELECT @ReservasConNewId = COUNT(*) FROM Reservas WHERE UserId IS NOT NULL
SELECT @ReservasSinId = COUNT(*) FROM Reservas WHERE UsuarioId IS NULL AND UserId IS NULL

PRINT '=== ESTADÍSTICAS PRE-MIGRACIÓN ==='
PRINT 'Registros en tabla de mapeo: ' + CAST(@MappingRecords AS VARCHAR(10))
PRINT ''
PRINT 'VISITAS:'
PRINT '  Con UsuarioId legacy: ' + CAST(@VisitasConLegacyId AS VARCHAR(10))
PRINT '  Con UserId nuevo: ' + CAST(@VisitasConNewId AS VARCHAR(10))
PRINT '  Sin usuario asignado: ' + CAST(@VisitasSinId AS VARCHAR(10))
PRINT ''
PRINT 'RESERVAS:'
PRINT '  Con UsuarioId legacy: ' + CAST(@ReservasConLegacyId AS VARCHAR(10))
PRINT '  Con UserId nuevo: ' + CAST(@ReservasConNewId AS VARCHAR(10))
PRINT '  Sin usuario asignado: ' + CAST(@ReservasSinId AS VARCHAR(10))

-- =============================================
-- PASO 3: Migrar Visitas
-- =============================================

PRINT ''
PRINT '=== MIGRANDO TABLA VISITAS ==='

BEGIN TRY
    BEGIN TRANSACTION MigrateVisitas
    
    DECLARE @VisitasMigradas INT = 0
    DECLARE @VisitasErrores INT = 0
    
    -- Migrar Visitas que tienen UsuarioId legacy pero no UserId nuevo
    UPDATE v
    SET UserId = m.AspNetUserId
    FROM Visitas v
    INNER JOIN LegacyUser_AspNetUser_Mapping m ON v.UsuarioId = m.LegacyUsuarioId
    WHERE v.UserId IS NULL
    AND v.UsuarioId IS NOT NULL
    
    SET @VisitasMigradas = @@ROWCOUNT
    
    -- Registrar migraciones exitosas en log
    DECLARE @LogVisitasSQL NVARCHAR(MAX) = 
        'INSERT INTO ' + @LogTable + ' (TableName, RecordId, LegacyUsuarioId, NewUserId, MigrationStatus)
         SELECT ''Visitas'', v.VisitaId, v.UsuarioId, v.UserId, ''SUCCESS''
         FROM Visitas v
         INNER JOIN LegacyUser_AspNetUser_Mapping m ON v.UsuarioId = m.LegacyUsuarioId
         WHERE v.UserId IS NOT NULL'
    
    EXEC sp_executesql @LogVisitasSQL
    
    -- Identificar registros con UsuarioId que no tienen mapeo
    DECLARE @LogVisitasErrorSQL NVARCHAR(MAX) = 
        'INSERT INTO ' + @LogTable + ' (TableName, RecordId, LegacyUsuarioId, NewUserId, MigrationStatus, ErrorMessage)
         SELECT ''Visitas'', v.VisitaId, v.UsuarioId, NULL, ''ERROR'', ''Legacy UsuarioId not found in mapping table''
         FROM Visitas v
         LEFT JOIN LegacyUser_AspNetUser_Mapping m ON v.UsuarioId = m.LegacyUsuarioId
         WHERE v.UsuarioId IS NOT NULL 
         AND v.UserId IS NULL
         AND m.LegacyUsuarioId IS NULL'
    
    EXEC sp_executesql @LogVisitasErrorSQL
    SET @VisitasErrores = @@ROWCOUNT
    
    COMMIT TRANSACTION MigrateVisitas
    
    PRINT 'Visitas migradas exitosamente: ' + CAST(@VisitasMigradas AS VARCHAR(10))
    PRINT 'Visitas con errores (sin mapeo): ' + CAST(@VisitasErrores AS VARCHAR(10))
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION MigrateVisitas
    PRINT 'ERROR migrando Visitas: ' + ERROR_MESSAGE()
    RETURN
END CATCH

-- =============================================
-- PASO 4: Migrar Reservas
-- =============================================

PRINT ''
PRINT '=== MIGRANDO TABLA RESERVAS ==='

BEGIN TRY
    BEGIN TRANSACTION MigrateReservas
    
    DECLARE @ReservasMigradas INT = 0
    DECLARE @ReservasErrores INT = 0
    
    -- Migrar Reservas que tienen UsuarioId legacy pero no UserId nuevo
    UPDATE r
    SET UserId = m.AspNetUserId
    FROM Reservas r
    INNER JOIN LegacyUser_AspNetUser_Mapping m ON r.UsuarioId = m.LegacyUsuarioId
    WHERE r.UserId IS NULL
    AND r.UsuarioId IS NOT NULL
    
    SET @ReservasMigradas = @@ROWCOUNT
    
    -- Registrar migraciones exitosas en log
    DECLARE @LogReservasSQL NVARCHAR(MAX) = 
        'INSERT INTO ' + @LogTable + ' (TableName, RecordId, LegacyUsuarioId, NewUserId, MigrationStatus)
         SELECT ''Reservas'', r.ReservaId, r.UsuarioId, r.UserId, ''SUCCESS''
         FROM Reservas r
         INNER JOIN LegacyUser_AspNetUser_Mapping m ON r.UsuarioId = m.LegacyUsuarioId
         WHERE r.UserId IS NOT NULL'
    
    EXEC sp_executesql @LogReservasSQL
    
    -- Identificar registros con UsuarioId que no tienen mapeo
    DECLARE @LogReservasErrorSQL NVARCHAR(MAX) = 
        'INSERT INTO ' + @LogTable + ' (TableName, RecordId, LegacyUsuarioId, NewUserId, MigrationStatus, ErrorMessage)
         SELECT ''Reservas'', r.ReservaId, r.UsuarioId, NULL, ''ERROR'', ''Legacy UsuarioId not found in mapping table''
         FROM Reservas r
         LEFT JOIN LegacyUser_AspNetUser_Mapping m ON r.UsuarioId = m.LegacyUsuarioId
         WHERE r.UsuarioId IS NOT NULL 
         AND r.UserId IS NULL
         AND m.LegacyUsuarioId IS NULL'
    
    EXEC sp_executesql @LogReservasErrorSQL
    SET @ReservasErrores = @@ROWCOUNT
    
    COMMIT TRANSACTION MigrateReservas
    
    PRINT 'Reservas migradas exitosamente: ' + CAST(@ReservasMigradas AS VARCHAR(10))
    PRINT 'Reservas con errores (sin mapeo): ' + CAST(@ReservasErrores AS VARCHAR(10))
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION MigrateReservas
    PRINT 'ERROR migrando Reservas: ' + ERROR_MESSAGE()
    RETURN
END CATCH

-- =============================================
-- PASO 5: Verificar consistencia post-migración
-- =============================================

PRINT ''
PRINT '=== VERIFICACIÓN POST-MIGRACIÓN ==='

-- Verificar consistencia entre Reservas y Visitas
DECLARE @InconsistentAfterMigration INT
SELECT @InconsistentAfterMigration = COUNT(*)
FROM Reservas r
INNER JOIN Visitas v ON r.VisitaId = v.VisitaId
WHERE r.UserId IS NOT NULL 
AND v.UserId IS NOT NULL 
AND r.UserId != v.UserId

IF @InconsistentAfterMigration > 0
BEGIN
    PRINT 'WARNING: ' + CAST(@InconsistentAfterMigration AS VARCHAR(10)) + ' reservas con UserId diferente al de su visita asociada después de la migración'
END
ELSE
BEGIN
    PRINT 'OK: Consistencia entre Reservas y Visitas verificada'
END

-- Mostrar estadísticas finales
SELECT @VisitasConNewId = COUNT(*) FROM Visitas WHERE UserId IS NOT NULL
SELECT @ReservasConNewId = COUNT(*) FROM Reservas WHERE UserId IS NOT NULL

PRINT ''
PRINT '=== ESTADÍSTICAS POST-MIGRACIÓN ==='
PRINT 'Visitas con UserId: ' + CAST(@VisitasConNewId AS VARCHAR(10))
PRINT 'Reservas con UserId: ' + CAST(@ReservasConNewId AS VARCHAR(10))

-- Mostrar resumen del log
DECLARE @ShowLogSQL NVARCHAR(MAX) = 
    'SELECT 
        TableName,
        MigrationStatus,
        COUNT(*) as RecordCount
     FROM ' + @LogTable + '
     GROUP BY TableName, MigrationStatus
     ORDER BY TableName, MigrationStatus'

PRINT ''
PRINT '=== RESUMEN DE MIGRACIÓN ==='
EXEC sp_executesql @ShowLogSQL

PRINT ''
PRINT 'Migración completada exitosamente!'
PRINT 'Tabla de log creada: ' + @LogTable
PRINT ''
PRINT 'PRÓXIMOS PASOS:'
PRINT '1. Verificar que la aplicación funciona correctamente con los nuevos UserId'
PRINT '2. Considerar deprecar los campos UsuarioId legacy'
PRINT '3. Actualizar consultas y procedimientos para usar UserId en lugar de UsuarioId'

GO