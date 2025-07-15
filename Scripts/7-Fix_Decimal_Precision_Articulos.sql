-- Script to fix decimal precision for Precio column in Articulos table
-- This addresses the arithmetic overflow error when saving decimal values

USE [Hotel_DB]; -- Replace with your actual database name
GO

-- Check current column definition
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Articulos' AND COLUMN_NAME = 'Precio';

-- Backup current data (optional - uncomment if needed)
-- SELECT * INTO Articulos_Backup FROM Articulos;

-- Fix the Precio column precision to DECIMAL(18,2)
-- This matches the Entity Framework configuration
EXEC('
    -- Check if column exists and has wrong precision
    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = ''Articulos'' 
        AND COLUMN_NAME = ''Precio''
        AND (NUMERIC_PRECISION != 18 OR NUMERIC_SCALE != 2)
    )
    BEGIN
        PRINT ''Updating Precio column precision to DECIMAL(18,2)...''
        
        -- Alter the column to the correct precision
        ALTER TABLE Articulos 
        ALTER COLUMN Precio DECIMAL(18,2) NOT NULL;
        
        PRINT ''Precio column updated successfully.''
    END
    ELSE
    BEGIN
        PRINT ''Precio column already has correct precision DECIMAL(18,2).''
    END
');

-- Verify the change
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Articulos' AND COLUMN_NAME = 'Precio';

-- Also check for any existing values that might be problematic
SELECT 
    ArticuloId,
    NombreArticulo,
    Precio,
    CASE 
        WHEN Precio > 9999999999999999.99 THEN 'Value too large'
        WHEN Precio < 0 THEN 'Negative value'
        ELSE 'OK'
    END AS ValidationStatus
FROM Articulos
WHERE Precio > 9999999999999999.99 OR Precio < 0;

-- If there are problematic values, you might need to update them:
-- UPDATE Articulos SET Precio = 999999.99 WHERE Precio > 9999999999999999.99;

PRINT 'Decimal precision fix completed. Check the query results above for verification.';