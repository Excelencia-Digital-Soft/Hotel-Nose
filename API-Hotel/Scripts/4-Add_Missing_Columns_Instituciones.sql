-- Script para agregar columnas faltantes a la tabla Instituciones
-- y luego insertar los registros de Hotel Nose y Hotel Taos

USE Hotel;
GO

-- Agregar columnas faltantes si no existen
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Instituciones') AND name = 'Direccion')
BEGIN
    ALTER TABLE Instituciones ADD Direccion NVARCHAR(500) NULL;
    PRINT 'Columna Direccion agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Instituciones') AND name = 'Telefono')
BEGIN
    ALTER TABLE Instituciones ADD Telefono NVARCHAR(50) NULL;
    PRINT 'Columna Telefono agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Instituciones') AND name = 'Email')
BEGIN
    ALTER TABLE Instituciones ADD Email NVARCHAR(100) NULL;
    PRINT 'Columna Email agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Instituciones') AND name = 'Estado')
BEGIN
    ALTER TABLE Instituciones ADD Estado NVARCHAR(50) NULL;
    PRINT 'Columna Estado agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Instituciones') AND name = 'FechaCreacion')
BEGIN
    ALTER TABLE Instituciones ADD FechaCreacion DATETIME NULL;
    PRINT 'Columna FechaCreacion agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Instituciones') AND name = 'Descripcion')
BEGIN
    ALTER TABLE Instituciones ADD Descripcion NVARCHAR(MAX) NULL;
    PRINT 'Columna Descripcion agregada';
END

GO

-- Ahora insertar o actualizar los datos
SET IDENTITY_INSERT Instituciones ON;

-- Hotel No Se (ID=1)
IF EXISTS (SELECT 1 FROM Instituciones WHERE InstitucionId = 1)
BEGIN
    UPDATE Instituciones SET
        Nombre = 'Hotel No Se',
        Direccion = 'Cmte. Cabot Oeste 1293, J5402 San Juan, Argentina',
        Telefono = '0264 15-585-4502',
        Email = 'reservas@hotelnoSe.com.ar',
        Estado = 'Activo',
        FechaCreacion = ISNULL(FechaCreacion, GETDATE()),
        Descripcion = 'Motel - Hotel de parejas en San Juan, Argentina'
    WHERE InstitucionId = 1;
    
    PRINT 'Hotel No Se actualizado correctamente (ID=1)';
END
ELSE
BEGIN
    INSERT INTO Instituciones (
        InstitucionId,
        Nombre,
        Direccion,
        Telefono,
        Email,
        Estado,
        FechaCreacion,
        Descripcion
    )
    VALUES (
        1,
        'Hotel No Se',
        'Cmte. Cabot Oeste 1293, J5402 San Juan, Argentina',
        '0264 15-585-4502',
        'reservas@hotelnoSe.com.ar',
        'Activo',
        GETDATE(),
        'Motel - Hotel de parejas en San Juan, Argentina'
    );
    
    PRINT 'Hotel No Se insertado correctamente (ID=1)';
END

-- Hotel Taos (ID=2)
IF EXISTS (SELECT 1 FROM Instituciones WHERE InstitucionId = 2)
BEGIN
    UPDATE Instituciones SET
        Nombre = 'Hotel Taos',
        Direccion = 'Carlos Pellegrini 1053 sur, Rivadavia, San Juan, Argentina',
        Telefono = '(0264) 154683297',
        Email = 'info@hoteltaos.com.ar',
        Estado = 'Activo',
        FechaCreacion = ISNULL(FechaCreacion, GETDATE()),
        Descripcion = 'Hotel con 18 habitaciones, 6 con jacuzzis, enfocado en privacidad y comodidad'
    WHERE InstitucionId = 2;
    
    PRINT 'Hotel Taos actualizado correctamente (ID=2)';
END
ELSE
BEGIN
    INSERT INTO Instituciones (
        InstitucionId,
        Nombre,
        Direccion,
        Telefono,
        Email,
        Estado,
        FechaCreacion,
        Descripcion
    )
    VALUES (
        2,
        'Hotel Taos',
        'Carlos Pellegrini 1053 sur, Rivadavia, San Juan, Argentina',
        '(0264) 154683297',
        'info@hoteltaos.com.ar',
        'Activo',
        GETDATE(),
        'Hotel con 18 habitaciones, 6 con jacuzzis, enfocado en privacidad y comodidad'
    );
    
    PRINT 'Hotel Taos insertado correctamente (ID=2)';
END

SET IDENTITY_INSERT Instituciones OFF;

-- Verificar los registros insertados
SELECT * FROM Instituciones WHERE InstitucionId IN (1, 2);

GO