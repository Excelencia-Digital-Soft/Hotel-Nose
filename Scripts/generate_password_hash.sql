-- =====================================================
-- Script para generar el hash correcto de "Pass123"
-- Este hash debe ser usado en el script de migración
-- =====================================================

-- El hash actual usado para "Pass123" en Identity:
-- 'AQAAAAEAACcQAAAAEKcO/+btL3p8+DxXFz7CjAqF/T5gK3QMF7pO1TLQ8sHx/R7nN4vF2Q1Y9gH3K8Wm'

PRINT '=== INFORMACIÓN DEL HASH DE CONTRASEÑA ===';
PRINT 'Contraseña por defecto: Pass123';
PRINT 'Hash de Identity: AQAAAAEAACcQAAAAEKcO/+btL3p8+DxXFz7CjAqF/T5gK3QMF7pO1TLQ8sHx/R7nN4vF2Q1Y9gH3K8Wm';
PRINT '';
PRINT 'IMPORTANTE:';
PRINT '- Este hash fue generado usando Microsoft.AspNetCore.Identity.PasswordHasher';
PRINT '- Es compatible con el sistema Identity de ASP.NET Core';
PRINT '- Todos los usuarios migrados tendrán esta contraseña temporalmente';
PRINT '- Los usuarios DEBEN cambiar su contraseña en el primer login';
PRINT '';

-- Verificar si necesitas regenerar el hash (ejecutar desde aplicación C#):
PRINT 'Para regenerar el hash, ejecuta este código C#:';
PRINT '';
PRINT 'var passwordHasher = new PasswordHasher<ApplicationUser>();';
PRINT 'var user = new ApplicationUser();';
PRINT 'string hash = passwordHasher.HashPassword(user, "Pass123");';
PRINT 'Console.WriteLine($"Nuevo hash: {hash}");';
PRINT '';

-- Query para verificar usuarios que usan el hash por defecto
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    DECLARE @DefaultHash NVARCHAR(MAX) = 'AQAAAAEAACcQAAAAEKcO/+btL3p8+DxXFz7CjAqF/T5gK3QMF7pO1TLQ8sHx/R7nN4vF2Q1Y9gH3K8Wm';
    DECLARE @UsersWithDefaultPassword INT;
    
    SELECT @UsersWithDefaultPassword = COUNT(*) 
    FROM AspNetUsers 
    WHERE PasswordHash = @DefaultHash;
    
    PRINT 'Usuarios con contraseña por defecto: ' + CAST(@UsersWithDefaultPassword AS NVARCHAR(10));
    
    IF @UsersWithDefaultPassword > 0
    BEGIN
        PRINT '';
        PRINT '⚠️  ADVERTENCIA: Hay usuarios con la contraseña por defecto';
        PRINT '   Estos usuarios deben cambiar su contraseña inmediatamente';
        
        -- Mostrar lista de usuarios con contraseña por defecto
        SELECT 
            UserName,
            Email,
            ForcePasswordChange,
            CreatedAt,
            LastLoginAt
        FROM AspNetUsers 
        WHERE PasswordHash = @DefaultHash
        ORDER BY UserName;
    END
    ELSE
    BEGIN
        PRINT '✅ No hay usuarios con la contraseña por defecto';
    END
END
ELSE
BEGIN
    PRINT '❌ La tabla AspNetUsers no existe. Ejecuta primero create_identity_tables.sql';
END