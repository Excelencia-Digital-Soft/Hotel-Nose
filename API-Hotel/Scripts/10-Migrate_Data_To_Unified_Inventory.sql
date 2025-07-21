-- =====================================================
-- Script: 10-Migrate_Data_To_Unified_Inventory.sql
-- Description: Migrates data from Inventarios and InventarioGeneral to InventarioUnificado
-- Author: Claude Code Assistant
-- Date: 2025-01-21
-- =====================================================

PRINT '======================================================'
PRINT 'MIGRATING DATA TO UNIFIED INVENTORY TABLE'
PRINT '======================================================'

-- STEP 1: Clear existing data in unified table (if any)
IF EXISTS (SELECT 1 FROM InventarioUnificado)
BEGIN
    PRINT 'Clearing existing data from InventarioUnificado...'
    DELETE FROM InventarioUnificado
    PRINT CONCAT('Cleared ', @@ROWCOUNT, ' existing records.')
END

-- STEP 2: Migrate room inventory (Inventarios)
PRINT ''
PRINT 'Migrating room inventory (Inventarios)...'

INSERT INTO [dbo].[InventarioUnificado] (
    [ArticuloId], [Cantidad], [InstitucionID], [TipoUbicacion], [UbicacionId],
    [FechaRegistro], [Anulado], [UsuarioRegistro], [Notas]
)
SELECT 
    i.[ArticuloId],
    i.[Cantidad],
    i.[InstitucionID],
    1 as [TipoUbicacion], -- Room type
    i.[HabitacionId] as [UbicacionId],
    COALESCE(i.[FechaRegistro], GETDATE()) as [FechaRegistro],
    i.[Anulado],
    NULL as [UsuarioRegistro], -- Legacy data doesn't have user tracking
    'Migrated from Inventarios table' as [Notas]
FROM [dbo].[Inventarios] i
WHERE i.[ArticuloId] IS NOT NULL
ORDER BY i.[InventarioId]

PRINT CONCAT('Migrated ', @@ROWCOUNT, ' room inventory records.')

-- STEP 3: Migrate general inventory (InventarioGeneral)
PRINT ''
PRINT 'Migrating general inventory (InventarioGeneral)...'

INSERT INTO [dbo].[InventarioUnificado] (
    [ArticuloId], [Cantidad], [InstitucionID], [TipoUbicacion], [UbicacionId],
    [FechaRegistro], [Anulado], [UsuarioRegistro], [Notas]
)
SELECT 
    ig.[ArticuloId],
    ig.[Cantidad],
    ig.[InstitucionID],
    0 as [TipoUbicacion], -- General type
    NULL as [UbicacionId],
    COALESCE(ig.[FechaRegistro], GETDATE()) as [FechaRegistro],
    ig.[Anulado],
    NULL as [UsuarioRegistro], -- Legacy data doesn't have user tracking
    'Migrated from InventarioGeneral table' as [Notas]
FROM [dbo].[InventarioGeneral] ig
WHERE ig.[ArticuloId] IS NOT NULL
ORDER BY ig.[InventarioId]

PRINT CONCAT('Migrated ', @@ROWCOUNT, ' general inventory records.')

-- STEP 4: Verify migration
PRINT ''
PRINT 'Verifying migration results...'

SELECT 
    'Migration Summary' as [Status],
    (SELECT COUNT(*) FROM [dbo].[Inventarios] WHERE [Anulado] IS NULL OR [Anulado] = 0) as [Original_Room_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioGeneral] WHERE [Anulado] IS NULL OR [Anulado] = 0) as [Original_General_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioUnificado] WHERE [TipoUbicacion] = 1 AND ([Anulado] IS NULL OR [Anulado] = 0)) as [Migrated_Room_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioUnificado] WHERE [TipoUbicacion] = 0 AND ([Anulado] IS NULL OR [Anulado] = 0)) as [Migrated_General_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioUnificado] WHERE [Anulado] IS NULL OR [Anulado] = 0) as [Total_Unified_Records]

-- STEP 5: Show sample data from unified table
PRINT ''
PRINT 'Sample data from InventarioUnificado:'

SELECT TOP 10
    iu.InventarioId,
    iu.ArticuloId,
    a.NombreArticulo,
    iu.Cantidad,
    CASE iu.TipoUbicacion 
        WHEN 0 THEN 'General'
        WHEN 1 THEN 'Room'
        WHEN 2 THEN 'Warehouse'
        ELSE 'Unknown'
    END as TipoUbicacionNombre,
    iu.UbicacionId,
    h.NombreHabitacion,
    iu.InstitucionID,
    iu.FechaRegistro
FROM [dbo].[InventarioUnificado] iu
LEFT JOIN [dbo].[Articulos] a ON iu.ArticuloId = a.ArticuloId
LEFT JOIN [dbo].[Habitaciones] h ON iu.UbicacionId = h.HabitacionId
ORDER BY iu.InventarioId

-- STEP 6: Validate data integrity
PRINT ''
PRINT 'Validating data integrity...'

-- Check for orphaned articles
SELECT 
    'Orphaned Articles Check' as CheckType,
    COUNT(*) as OrphanedCount
FROM [dbo].[InventarioUnificado] iu
LEFT JOIN [dbo].[Articulos] a ON iu.ArticuloId = a.ArticuloId
WHERE a.ArticuloId IS NULL

-- Check for orphaned rooms
SELECT 
    'Orphaned Rooms Check' as CheckType,
    COUNT(*) as OrphanedCount
FROM [dbo].[InventarioUnificado] iu
LEFT JOIN [dbo].[Habitaciones] h ON iu.UbicacionId = h.HabitacionId
WHERE iu.TipoUbicacion = 1 AND iu.UbicacionId IS NOT NULL AND h.HabitacionId IS NULL

-- Check for data consistency
SELECT 
    'Data Consistency Check' as CheckType,
    COUNT(*) as InconsistentCount
FROM [dbo].[InventarioUnificado] iu
WHERE (iu.TipoUbicacion = 1 AND iu.UbicacionId IS NULL) OR 
      (iu.TipoUbicacion = 0 AND iu.UbicacionId IS NOT NULL)

PRINT ''
PRINT '======================================================'
PRINT 'MIGRATION COMPLETED SUCCESSFULLY'
PRINT '======================================================'
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. Test the V1 API endpoints to ensure they work with unified table'
PRINT '2. Update any remaining code to use InventoryUnifiedService'
PRINT '3. Verify all functionality works correctly'
PRINT '4. FUTURE: Consider archiving old tables once fully migrated'
PRINT ''
PRINT 'IMPORTANT NOTES:'
PRINT '- Legacy controllers still use old tables for backward compatibility'
PRINT '- V1 API now uses the unified InventarioUnificado table'
PRINT '- Both systems can coexist during transition period'
PRINT '======================================================'