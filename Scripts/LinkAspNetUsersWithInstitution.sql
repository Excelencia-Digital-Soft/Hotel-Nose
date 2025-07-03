-- Script para vincular AspNetUsers con la tabla Institucion
-- Este script debe ejecutarse después de crear las tablas de Identity

-- 1. Agregar la columna InstitucionId si no existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'InstitucionId')
BEGIN
    ALTER TABLE AspNetUsers
    ADD InstitucionId INT NULL;
END

-- 2. Crear la foreign key constraint hacia la tabla Instituciones
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
               WHERE CONSTRAINT_NAME = 'FK_AspNetUsers_Instituciones_InstitucionId')
BEGIN
    ALTER TABLE AspNetUsers
    ADD CONSTRAINT FK_AspNetUsers_Instituciones_InstitucionId
    FOREIGN KEY (InstitucionId) REFERENCES Instituciones(InstitucionId);
END

-- 3. Crear índice para mejorar rendimiento en consultas por institución
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_AspNetUsers_InstitucionId' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE INDEX IX_AspNetUsers_InstitucionId 
    ON AspNetUsers (InstitucionId);
END

-- 4. Opcional: Actualizar usuarios existentes que no tienen institución asignada
-- Asignar la primera institución disponible a usuarios sin institución
-- (Comentado por seguridad - descomentar solo si es necesario)
/*
UPDATE AspNetUsers 
SET InstitucionId = (SELECT TOP 1 InstitucionId FROM Instituciones WHERE Estado = 'Activo')
WHERE InstitucionId IS NULL;
*/

-- 5. Verificar la configuración
SELECT 
    'Configuración completada' as Status,
    COUNT(*) as TotalUsers,
    COUNT(InstitucionId) as UsersWithInstitution,
    COUNT(*) - COUNT(InstitucionId) as UsersWithoutInstitution
FROM AspNetUsers;

-- 6. Mostrar usuarios y sus instituciones
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.InstitucionId,
    i.Nombre as InstitucionNombre
FROM AspNetUsers u
LEFT JOIN Instituciones i ON u.InstitucionId = i.InstitucionId
ORDER BY u.UserName;

PRINT 'Script de vinculación AspNetUsers-Institucion ejecutado correctamente';