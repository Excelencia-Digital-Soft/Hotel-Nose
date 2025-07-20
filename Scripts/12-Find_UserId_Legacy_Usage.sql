-- =============================================
-- Script: 12-Find_UserId_Legacy_Usage.sql
-- Description: Script para identificar patrones legacy de UserId 
--              que necesitan corrección en el código
-- Author: Claude Code Assistant
-- Date: 2025-07-19
-- Version: 1.0
-- =============================================

/*
Este script SQL está diseñado para identificar registros en la base de datos
que tienen datos en campos legacy que necesitan migración.

Para buscar patrones de código problemáticos, ejecutar estos comandos en el terminal:

# 1. Buscar patrones de int.TryParse con UserId
grep -r "int\.TryParse.*[Uu]ser" API-Hotel/ --include="*.cs"

# 2. Buscar métodos que reciben userId como int
grep -r "int userId" API-Hotel/ --include="*.cs"

# 3. Buscar ClaimTypes.NameIdentifier con TryParse
grep -r "ClaimTypes\.NameIdentifier" API-Hotel/ --include="*.cs" -A 3 | grep -B 3 -A 3 "int\.TryParse"

# 4. Buscar métodos de servicios que reciben int userId
grep -r "Task.*int.*userId" API-Hotel/Services/ --include="*.cs"
grep -r "Task.*userId.*int" API-Hotel/Services/ --include="*.cs"

# 5. Buscar logs que usan userId como int
grep -r "LogInformation.*userId" API-Hotel/ --include="*.cs"

# 6. Buscar constructores o métodos de modelo con UsuarioId (legacy)
grep -r "\.UsuarioId\s*=" API-Hotel/ --include="*.cs"

PATRONES A CORREGIR:

1. CONTROLADORES:
   CAMBIAR: var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
   
   POR:     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
   
   O MEJOR: var currentUser = this.GetCurrentUserInfo();
            if (currentUser == null)

2. INTERFACES DE SERVICIOS:
   CAMBIAR: Task<ApiResponse> MetodoAsync(..., int userId, ...)
   POR:     Task<ApiResponse> MetodoAsync(..., string userId, ...)

3. IMPLEMENTACIONES DE SERVICIOS:
   CAMBIAR: public async Task<ApiResponse> MetodoAsync(..., int userId, ...)
   POR:     public async Task<ApiResponse> MetodoAsync(..., string userId, ...)

4. ASIGNACIONES A MODELOS:
   CAMBIAR: entidad.UsuarioId = userId; // int legacy
   POR:     entidad.UserId = userId;    // string nuevo

5. LOGS:
   MANTENER: _logger.LogInformation("... {UserId}", userId);
   (Los logs funcionan igual, solo cambia el tipo de userId)

6. AUDITORÍA:
   USAR:    await _registrosService.LogAuditAsync(
                "...", 
                ModuloSistema.MODULO,
                institucionId,
                userId,      // string UserId de Identity
                ipAddress,   // Obtener con this.GetClientIpAddress()
                jsonDetails,
                relacionId);
*/

-- Verificar datos legacy en base de datos
USE [HotelDB]
GO

PRINT '=== VERIFICACIÓN DE DATOS LEGACY DE USUARIOS ==='

-- 1. Verificar registros con UsuarioId legacy en Visitas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Visitas')
BEGIN
    DECLARE @VisitasLegacy INT
    SELECT @VisitasLegacy = COUNT(*) FROM Visitas WHERE UsuarioId IS NOT NULL AND UserId IS NULL
    PRINT 'Visitas con UsuarioId legacy sin migrar: ' + CAST(@VisitasLegacy AS VARCHAR(10))
END

-- 2. Verificar registros con UsuarioId legacy en Reservas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reservas')
BEGIN
    DECLARE @ReservasLegacy INT
    SELECT @ReservasLegacy = COUNT(*) FROM Reservas WHERE UsuarioId IS NOT NULL AND UserId IS NULL
    PRINT 'Reservas con UsuarioId legacy sin migrar: ' + CAST(@ReservasLegacy AS VARCHAR(10))
END

-- 3. Verificar registros con UsuarioId legacy en Registros (si tiene campo legacy)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Registros' AND COLUMN_NAME = 'UsuarioId')
BEGIN
    DECLARE @RegistrosLegacy INT
    SELECT @RegistrosLegacy = COUNT(*) FROM Registros WHERE UsuarioId IS NOT NULL AND UserId IS NULL
    PRINT 'Registros con UsuarioId legacy sin migrar: ' + CAST(@RegistrosLegacy AS VARCHAR(10))
END

-- 4. Verificar usuarios en AspNetUsers vs sistema legacy
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
   AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuarios')
BEGIN
    DECLARE @AspNetUsers INT, @LegacyUsers INT
    SELECT @AspNetUsers = COUNT(*) FROM AspNetUsers WHERE IsActive = 1
    SELECT @LegacyUsers = COUNT(*) FROM Usuarios WHERE Anulado IS NULL OR Anulado = 0
    
    PRINT 'Usuarios en AspNetUsers (activos): ' + CAST(@AspNetUsers AS VARCHAR(10))
    PRINT 'Usuarios en tabla legacy Usuarios: ' + CAST(@LegacyUsers AS VARCHAR(10))
END

-- 5. Mostrar ejemplo de mapeo necesario
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
BEGIN
    PRINT ''
    PRINT '=== EJEMPLO DE USUARIOS EN ASPNETUSERS ==='
    SELECT TOP 5
        Id as AspNetUserId,
        UserName,
        Email,
        InstitucionId,
        IsActive
    FROM AspNetUsers
    ORDER BY CreatedAt DESC
END

PRINT ''
PRINT '=== PASOS PARA CORRECCIÓN ==='
PRINT '1. Ejecutar comandos grep mostrados arriba para encontrar código legacy'
PRINT '2. Cambiar métodos de controladores para usar string userId'
PRINT '3. Actualizar interfaces y servicios para usar string userId'
PRINT '4. Cambiar asignaciones de UsuarioId por UserId en modelos'
PRINT '5. Usar extension methods (GetCurrentUserId, GetCurrentUserInfo)'
PRINT '6. Migrar datos legacy usando script 11-Migrate_UsuarioId_To_UserId.sql'

GO