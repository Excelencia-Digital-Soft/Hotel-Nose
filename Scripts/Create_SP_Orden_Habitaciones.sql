-- 1. Agregar el campo 'Orden' a la tabla CategoriasHabitaciones si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CategoriasHabitaciones]') AND name = 'Orden')
BEGIN
    ALTER TABLE [dbo].[CategoriasHabitaciones] ADD [Orden] INT NULL;
END
GO

-- 2. Agregar el campo 'Numero' a la tabla Habitaciones si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Habitaciones]') AND name = 'Numero')
BEGIN
    ALTER TABLE [dbo].[Habitaciones] ADD [Numero] INT NULL;
END
GO

-- 3. Actualizar el orden para las categorías especificadas
UPDATE CategoriasHabitaciones SET Orden = 1 WHERE NombreCategoria LIKE '%CLÁSICA%' OR NombreCategoria LIKE '%CLASICA%';
UPDATE CategoriasHabitaciones SET Orden = 2 WHERE NombreCategoria = 'SUITE';
UPDATE CategoriasHabitaciones SET Orden = 3 WHERE NombreCategoria = 'MASTER SUITE';
UPDATE CategoriasHabitaciones SET Orden = 4 WHERE NombreCategoria = 'HIDRO SUITE';
UPDATE CategoriasHabitaciones SET Orden = 5 WHERE NombreCategoria = 'HIDROMAX SUITE';
UPDATE CategoriasHabitaciones SET Orden = 6 WHERE NombreCategoria = 'PENTHOUSE';
GO

-- 4. Crear el SP para listar las habitaciones con filtrado y orden específico
IF OBJECT_ID('sp_ListarHabitaciones', 'P') IS NOT NULL
    DROP PROCEDURE sp_ListarHabitaciones;
GO

CREATE PROCEDURE sp_ListarHabitaciones
    @CategoriaID INT = NULL, -- Parámetro para filtrar por categoría desde el desplegable
    @InstitucionID INT = NULL
AS
BEGIN
    SELECT 
        H.HabitacionID,
        H.Numero, -- Nuevo campo solicitado
        H.NombreHabitacion,
        H.CategoriaID,
        H.Disponible,
        H.Anulado,
        CH.NombreCategoria,
        CH.Orden AS CategoriaOrden
    FROM 
        Habitaciones H
    INNER JOIN 
        CategoriasHabitaciones CH ON H.CategoriaId = CH.CategoriaId
    WHERE 
        (@CategoriaID IS NULL OR H.CategoriaId = @CategoriaID)
        AND (@InstitucionID IS NULL OR H.InstitucionID = @InstitucionID)
        AND ISNULL(H.Anulado, 0) = 0
    ORDER BY 
        ISNULL(CH.Orden, 999) ASC, -- Primero por el orden de la categoría
        ISNULL(H.Numero, 0) ASC,   -- Luego por el número de habitación (nuevo campo)
        H.NombreHabitacion ASC     -- Finalmente por nombre si no hay número
END
GO
