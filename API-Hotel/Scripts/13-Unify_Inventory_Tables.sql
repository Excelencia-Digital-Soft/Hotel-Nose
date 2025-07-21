-- =====================================================
-- Script: 9-Unify_Inventory_Tables.sql
-- Description: Unifies Inventarios and InventarioGeneral tables into a single InventarioUnificado table
-- Author: Claude Code Assistant
-- Date: 2025-01-21
-- Note: This is an OPTIONAL migration - current system works with both tables
-- =====================================================

-- STEP 1: Create the unified inventory table
-- This table will replace both Inventarios and InventarioGeneral
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioUnificado')
BEGIN
    PRINT 'Creating InventarioUnificado table...'
    
    CREATE TABLE [dbo].[InventarioUnificado](
        [InventarioId] [int] IDENTITY(1,1) NOT NULL,
        [ArticuloId] [int] NOT NULL,
        [Cantidad] [int] NOT NULL DEFAULT 0,
        [InstitucionID] [int] NOT NULL,
        
        -- Location tracking
        [TipoUbicacion] [int] NOT NULL DEFAULT 0, -- 0 = General, 1 = Room, 2 = Warehouse
        [UbicacionId] [int] NULL, -- HabitacionID for room inventory, null for general
        
        -- Audit fields (improved)
        [FechaRegistro] [datetime2] NOT NULL DEFAULT GETDATE(),
        [FechaUltimaActualizacion] [datetime2] NULL,
        [UsuarioRegistro] [nvarchar](450) NULL, -- ASP.NET Identity UserId
        [UsuarioUltimaActualizacion] [nvarchar](450) NULL,
        
        -- Soft delete
        [Anulado] [bit] NULL DEFAULT 0,
        [FechaAnulacion] [datetime2] NULL,
        [UsuarioAnulacion] [nvarchar](450) NULL,
        [MotivoAnulacion] [nvarchar](200) NULL,
        
        -- Additional tracking
        [CantidadMinima] [int] NULL DEFAULT 0, -- Minimum stock level
        [CantidadMaxima] [int] NULL, -- Maximum stock level
        [PuntoReorden] [int] NULL DEFAULT 0, -- Reorder point
        [Notas] [nvarchar](500) NULL,
        
        CONSTRAINT [PK_InventarioUnificado] PRIMARY KEY CLUSTERED ([InventarioId] ASC),
        
        -- Foreign key constraints
        CONSTRAINT [FK_InventarioUnificado_Articulos] FOREIGN KEY ([ArticuloId])
            REFERENCES [dbo].[Articulos] ([ArticuloId]),
        CONSTRAINT [FK_InventarioUnificado_Instituciones] FOREIGN KEY ([InstitucionID])
            REFERENCES [dbo].[Instituciones] ([InstitucionID]),
        CONSTRAINT [FK_InventarioUnificado_Habitaciones] FOREIGN KEY ([UbicacionId])
            REFERENCES [dbo].[Habitaciones] ([HabitacionID]),
            
        -- Business constraints
        CONSTRAINT [CK_InventarioUnificado_Cantidad] CHECK ([Cantidad] >= 0),
        CONSTRAINT [CK_InventarioUnificado_TipoUbicacion] CHECK ([TipoUbicacion] IN (0, 1, 2)),
        CONSTRAINT [CK_InventarioUnificado_UbicacionRoom] CHECK (
            ([TipoUbicacion] = 1 AND [UbicacionId] IS NOT NULL) OR 
            ([TipoUbicacion] != 1 AND ([UbicacionId] IS NULL OR [TipoUbicacion] = 2))
        ),
        
        -- Unique constraint to prevent duplicates
        CONSTRAINT [UK_InventarioUnificado_Articulo_Ubicacion] UNIQUE ([ArticuloId], [TipoUbicacion], [UbicacionId], [InstitucionID])
    )
    
    PRINT 'InventarioUnificado table created successfully.'
END
ELSE
BEGIN
    PRINT 'InventarioUnificado table already exists.'
END

-- STEP 2: Create indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_InventarioUnificado_InstitucionID_TipoUbicacion')
BEGIN
    PRINT 'Creating performance indexes...'
    
    CREATE NONCLUSTERED INDEX [IX_InventarioUnificado_InstitucionID_TipoUbicacion] 
    ON [dbo].[InventarioUnificado] ([InstitucionID], [TipoUbicacion])
    INCLUDE ([ArticuloId], [Cantidad], [UbicacionId])
    
    CREATE NONCLUSTERED INDEX [IX_InventarioUnificado_ArticuloId_InstitucionID] 
    ON [dbo].[InventarioUnificado] ([ArticuloId], [InstitucionID])
    INCLUDE ([Cantidad], [TipoUbicacion], [UbicacionId])
    
    CREATE NONCLUSTERED INDEX [IX_InventarioUnificado_UbicacionId] 
    ON [dbo].[InventarioUnificado] ([UbicacionId])
    INCLUDE ([ArticuloId], [Cantidad], [InstitucionID])
    WHERE [UbicacionId] IS NOT NULL
    
    PRINT 'Indexes created successfully.'
END

-- STEP 3: Migrate data from existing tables (OPTIONAL - MANUAL EXECUTION REQUIRED)
-- UNCOMMENT THE FOLLOWING SECTION TO PERFORM DATA MIGRATION

PRINT 'Starting data migration...'

-- Migrate room inventory (Inventarios)
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
    i.[FechaRegistro],
    i.[Anulado],
    NULL as [UsuarioRegistro], -- Legacy data doesn't have user tracking
    'Migrated from Inventarios table' as [Notas]
FROM [dbo].[Inventarios] i
LEFT JOIN [dbo].[InventarioUnificado] iu ON (
    iu.[ArticuloId] = i.[ArticuloId] 
    AND iu.[TipoUbicacion] = 1 
    AND iu.[UbicacionId] = i.[HabitacionId] 
    AND iu.[InstitucionID] = i.[InstitucionID]
)
WHERE iu.[InventarioId] IS NULL -- Avoid duplicates
ORDER BY i.[InventarioId]

PRINT CONCAT('Migrated ', @@ROWCOUNT, ' room inventory records.')

-- Migrate general inventory (InventarioGeneral)
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
    ig.[FechaRegistro],
    ig.[Anulado],
    NULL as [UsuarioRegistro], -- Legacy data doesn't have user tracking
    'Migrated from InventarioGeneral table' as [Notas]
FROM [dbo].[InventarioGeneral] ig
LEFT JOIN [dbo].[InventarioUnificado] iu ON (
    iu.[ArticuloId] = ig.[ArticuloId] 
    AND iu.[TipoUbicacion] = 0 
    AND iu.[UbicacionId] IS NULL 
    AND iu.[InstitucionID] = ig.[InstitucionID]
)
WHERE iu.[InventarioId] IS NULL -- Avoid duplicates
ORDER BY ig.[InventarioId]

PRINT CONCAT('Migrated ', @@ROWCOUNT, ' general inventory records.')

-- Verify migration
SELECT 
    'Migration Summary' as [Status],
    (SELECT COUNT(*) FROM [dbo].[Inventarios] WHERE [Anulado] IS NULL OR [Anulado] = 0) as [Original_Room_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioGeneral] WHERE [Anulado] IS NULL OR [Anulado] = 0) as [Original_General_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioUnificado] WHERE [TipoUbicacion] = 1 AND ([Anulado] IS NULL OR [Anulado] = 0)) as [Migrated_Room_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioUnificado] WHERE [TipoUbicacion] = 0 AND ([Anulado] IS NULL OR [Anulado] = 0)) as [Migrated_General_Records],
    (SELECT COUNT(*) FROM [dbo].[InventarioUnificado] WHERE [Anulado] IS NULL OR [Anulado] = 0) as [Total_Unified_Records]

PRINT 'Data migration completed successfully.'

-- STEP 4: Create views for backward compatibility (OPTIONAL)
-- These views maintain compatibility with existing legacy code

IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_InventariosCompatibility')
BEGIN
    PRINT 'Creating compatibility views...'
    
    -- View to emulate Inventarios table
    EXEC('CREATE VIEW [dbo].[vw_InventariosCompatibility] AS
    SELECT 
        [InventarioId],
        [ArticuloId],
        [Cantidad],
        [UbicacionId] as [HabitacionId],
        [InstitucionID],
        [FechaRegistro],
        [Anulado]
    FROM [dbo].[InventarioUnificado]
    WHERE [TipoUbicacion] = 1 -- Room inventory only
    ')
    
    -- View to emulate InventarioGeneral table
    EXEC('CREATE VIEW [dbo].[vw_InventarioGeneralCompatibility] AS
    SELECT 
        [InventarioId],
        [ArticuloId],
        [Cantidad],
        [InstitucionID],
        [FechaRegistro],
        [Anulado]
    FROM [dbo].[InventarioUnificado]
    WHERE [TipoUbicacion] = 0 -- General inventory only
    ')
    
    PRINT 'Compatibility views created successfully.'
END

-- STEP 5: Create stored procedures for common inventory operations
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetInventoryByLocation')
BEGIN
    PRINT 'Creating stored procedures...'
    
    -- Get inventory by location type and ID
    EXEC('CREATE PROCEDURE [dbo].[sp_GetInventoryByLocation]
        @InstitucionID int,
        @TipoUbicacion int = NULL,
        @UbicacionId int = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            iu.[InventarioId],
            iu.[ArticuloId],
            a.[Nombre] as [ArticuloNombre],
            a.[Descripcion] as [ArticuloDescripcion],
            a.[Precio] as [ArticuloPrecio],
            iu.[Cantidad],
            iu.[TipoUbicacion],
            iu.[UbicacionId],
            CASE 
                WHEN iu.[TipoUbicacion] = 0 THEN ''Inventario General''
                WHEN iu.[TipoUbicacion] = 1 THEN COALESCE(h.[NombreHabitacion], ''Habitación'')
                WHEN iu.[TipoUbicacion] = 2 THEN ''Almacén''
                ELSE ''Desconocido''
            END as [UbicacionNombre],
            iu.[FechaRegistro],
            iu.[FechaUltimaActualizacion],
            iu.[CantidadMinima],
            iu.[CantidadMaxima],
            iu.[PuntoReorden],
            iu.[Notas],
            iu.[Anulado]
        FROM [dbo].[InventarioUnificado] iu
        INNER JOIN [dbo].[Articulos] a ON iu.[ArticuloId] = a.[ArticuloId]
        LEFT JOIN [dbo].[Habitaciones] h ON iu.[UbicacionId] = h.[HabitacionID]
        WHERE iu.[InstitucionID] = @InstitucionID
            AND (iu.[Anulado] IS NULL OR iu.[Anulado] = 0)
            AND (@TipoUbicacion IS NULL OR iu.[TipoUbicacion] = @TipoUbicacion)
            AND (@UbicacionId IS NULL OR iu.[UbicacionId] = @UbicacionId)
        ORDER BY iu.[TipoUbicacion], iu.[UbicacionId], a.[Nombre]
    END')
    
    PRINT 'Stored procedures created successfully.'
END

-- STEP 6: Create inventory movement tracking table (OPTIONAL - FUTURE USE)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioMovimientos')
BEGIN
    PRINT 'Creating inventory movement tracking table...'
    
    CREATE TABLE [dbo].[InventarioMovimientos](
        [MovimientoId] [int] IDENTITY(1,1) NOT NULL,
        [InventarioId] [int] NOT NULL,
        [ArticuloId] [int] NOT NULL,
        [TipoMovimiento] [int] NOT NULL, -- 0=Addition, 1=Removal, 2=Transfer, 3=Consumption, 4=Adjustment
        [CantidadAnterior] [int] NOT NULL,
        [CantidadMovimiento] [int] NOT NULL,
        [CantidadNueva] [int] NOT NULL,
        
        -- Location tracking
        [UbicacionOrigenTipo] [int] NULL,
        [UbicacionOrigenId] [int] NULL,
        [UbicacionDestinoTipo] [int] NULL,
        [UbicacionDestinoId] [int] NULL,
        
        -- Audit fields
        [FechaMovimiento] [datetime2] NOT NULL DEFAULT GETDATE(),
        [UsuarioId] [nvarchar](450) NULL,
        [InstitucionID] [int] NOT NULL,
        [Notas] [nvarchar](500) NULL,
        [ReferenciaId] [int] NULL, -- Reference to consumption, transfer, etc.
        [ReferenciaTabla] [nvarchar](50) NULL, -- Table name for reference
        
        CONSTRAINT [PK_InventarioMovimientos] PRIMARY KEY CLUSTERED ([MovimientoId] ASC),
        CONSTRAINT [FK_InventarioMovimientos_InventarioUnificado] FOREIGN KEY ([InventarioId])
            REFERENCES [dbo].[InventarioUnificado] ([InventarioId]),
        CONSTRAINT [FK_InventarioMovimientos_Articulos] FOREIGN KEY ([ArticuloId])
            REFERENCES [dbo].[Articulos] ([ArticuloId]),
        CONSTRAINT [FK_InventarioMovimientos_Instituciones] FOREIGN KEY ([InstitucionID])
            REFERENCES [dbo].[Instituciones] ([InstitucionID]),
        CONSTRAINT [CK_InventarioMovimientos_TipoMovimiento] CHECK ([TipoMovimiento] IN (0, 1, 2, 3, 4))
    )
    
    -- Index for movement history queries
    CREATE NONCLUSTERED INDEX [IX_InventarioMovimientos_InstitucionID_FechaMovimiento] 
    ON [dbo].[InventarioMovimientos] ([InstitucionID], [FechaMovimiento] DESC)
    INCLUDE ([ArticuloId], [TipoMovimiento], [CantidadMovimiento])
    
    CREATE NONCLUSTERED INDEX [IX_InventarioMovimientos_ArticuloId_FechaMovimiento] 
    ON [dbo].[InventarioMovimientos] ([ArticuloId], [FechaMovimiento] DESC)
    INCLUDE ([TipoMovimiento], [CantidadMovimiento], [UsuarioId])
    
    PRINT 'Inventory movement tracking table created successfully.'
END

-- STEP 7: Summary and next steps
PRINT '================================================'
PRINT 'INVENTORY UNIFICATION SCRIPT COMPLETED'
PRINT '================================================'
PRINT ''
PRINT 'WHAT WAS CREATED:'
PRINT '- InventarioUnificado table (unified inventory)'
PRINT '- Performance indexes'
PRINT '- Compatibility views (vw_InventariosCompatibility, vw_InventarioGeneralCompatibility)'
PRINT '- Stored procedures (sp_GetInventoryByLocation)'
PRINT '- InventarioMovimientos table (movement tracking)'
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. OPTIONAL: Uncomment and run the data migration section'
PRINT '2. Update application code to use new unified table'
PRINT '3. Test thoroughly with both old and new APIs'
PRINT '4. FUTURE: Remove old tables once migration is complete'
PRINT ''
PRINT 'BENEFITS:'
PRINT '- Single source of truth for all inventory'
PRINT '- Better performance with optimized indexes'
PRINT '- Enhanced audit trail and user tracking'
PRINT '- Support for future features (stock levels, movement tracking)'
PRINT '- Backward compatibility through views'
PRINT ''
PRINT 'IMPORTANT: This migration is OPTIONAL'
PRINT 'The current V1 API works with both table structures.'
PRINT '================================================'