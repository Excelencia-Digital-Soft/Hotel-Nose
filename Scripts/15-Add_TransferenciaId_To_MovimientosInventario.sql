-- Script: 15-Add_TransferenciaId_To_MovimientosInventario.sql
-- Description: Adds relationship between MovimientoInventario and TransferenciaInventario
-- Date: 2025-01-25

USE [Hotel]
GO

-- Add TransferenciaId column to MovimientosInventario table
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'MovimientosInventario' 
    AND COLUMN_NAME = 'TransferenciaId'
)
BEGIN
    PRINT 'Adding TransferenciaId column to MovimientosInventario table...'
    
    ALTER TABLE [dbo].[MovimientosInventario]
    ADD [TransferenciaId] int NULL
    
    PRINT 'TransferenciaId column added successfully.'
END
ELSE
BEGIN
    PRINT 'TransferenciaId column already exists in MovimientosInventario table.'
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME = 'FK_MovimientosInventario_TransferenciasInventario'
)
BEGIN
    -- Check if TransferenciasInventario table exists first
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransferenciasInventario')
    BEGIN
        PRINT 'Adding foreign key constraint FK_MovimientosInventario_TransferenciasInventario...'
        
        ALTER TABLE [dbo].[MovimientosInventario]
        ADD CONSTRAINT [FK_MovimientosInventario_TransferenciasInventario]
        FOREIGN KEY ([TransferenciaId]) 
        REFERENCES [dbo].[TransferenciasInventario] ([TransferenciaId])
        ON DELETE SET NULL
        
        PRINT 'Foreign key constraint added successfully.'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: TransferenciasInventario table does not exist. Foreign key constraint not created.'
    END
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_MovimientosInventario_TransferenciasInventario already exists.'
END
GO

-- Add index for performance if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_MovimientosInventario_TransferenciaId' 
    AND object_id = OBJECT_ID('MovimientosInventario')
)
BEGIN
    PRINT 'Creating index IX_MovimientosInventario_TransferenciaId...'
    
    CREATE NONCLUSTERED INDEX [IX_MovimientosInventario_TransferenciaId]
    ON [dbo].[MovimientosInventario] ([TransferenciaId])
    INCLUDE ([MovimientoId], [InventarioId], [TipoMovimiento], [FechaMovimiento])
    
    PRINT 'Index created successfully.'
END
ELSE
BEGIN
    PRINT 'Index IX_MovimientosInventario_TransferenciaId already exists.'
END
GO

-- Update any existing transfer movements to link them properly (optional)
-- This section is commented out as it requires business logic decisions

/*
-- Example: Link existing transfer movements based on timing and location matching
-- This is just an example and should be reviewed before execution

UPDATE mi
SET TransferenciaId = t.TransferenciaId
FROM MovimientosInventario mi
INNER JOIN TransferenciasInventario t ON (
    -- Link based on timing (movements within 1 hour of transfer completion)
    ABS(DATEDIFF(MINUTE, mi.FechaMovimiento, t.FechaCompletado)) <= 60
    AND mi.TipoMovimiento = 'Transferencia'
    AND t.Estado = 'Completada'
    -- Add more conditions as needed for your business logic
)
WHERE mi.TransferenciaId IS NULL
AND mi.TipoMovimiento = 'Transferencia'

PRINT 'Existing transfer movements linked to transfers where possible.'
*/

PRINT 'Script 15-Add_TransferenciaId_To_MovimientosInventario.sql completed successfully.'
GO