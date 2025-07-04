-- Script para insertar registros en la tabla Instituciones
-- Hotel Nose (ID=1) y Hotel Taos (ID=2)

USE Hotel;
GO

-- Habilitar IDENTITY_INSERT para poder insertar valores específicos de ID
SET IDENTITY_INSERT Instituciones ON;

-- Verificar si ya existen los registros antes de insertarlos
IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE InstitucionId = 1)
BEGIN
    INSERT INTO Instituciones (
        InstitucionId,
        Nombre,
        Direccion,
        Telefono,
        Email,
        Estado,
        FechaCreacion,
        Descripcion,
        TipoID,
        FechaAnulado
    )
    VALUES (
        1,
        'Hotel No Se',
        'Cmte. Cabot Oeste 1293, J5402 San Juan, Argentina',
        '0264 15-585-4502',
        'reservas@hotelnoSe.com.ar',
        'Activo',
        GETDATE(),
        'Motel - Hotel de parejas en San Juan, Argentina',
        1,
        NULL
    );
    
    PRINT 'Hotel Nose insertado correctamente (ID=1)';
END
ELSE
BEGIN
    PRINT 'Hotel Nose ya existe (ID=1)';
END

IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE InstitucionId = 2)
BEGIN
    INSERT INTO Instituciones (
        InstitucionId,
        Nombre,
        Direccion,
        Telefono,
        Email,
        Estado,
        FechaCreacion,
        Descripcion,
        TipoID,
        FechaAnulado
    )
    VALUES (
        2,
        'Hotel Taos',
        'Carlos Pellegrini 1053 sur, Rivadavia, San Juan, Argentina',
        '(0264) 154683297',
        'info@hoteltaos.com.ar',
        'Activo',
        GETDATE(),
        'Hotel con 18 habitaciones, 6 con jacuzzis, enfocado en privacidad y comodidad',
        1,
        NULL
    );
    
    PRINT 'Hotel Taos insertado correctamente (ID=2)';
END
ELSE
BEGIN
    PRINT 'Hotel Taos ya existe (ID=2)';
END

-- Deshabilitar IDENTITY_INSERT
SET IDENTITY_INSERT Instituciones OFF;

-- Verificar los registros insertados
SELECT * FROM Instituciones WHERE InstitucionId IN (1, 2);

GO