-- =============================================
-- Script: 9-Create_Inventory_Extended_Tables.sql
-- Description: Creates extended inventory management tables for movements, alerts, and transfers
-- Version: 1.0
-- Date: 2025-01-25
-- Author: Claude AI Assistant
-- =============================================

USE [Hotel]
GO

-- =============================================
-- 1. MOVIMIENTOS DE INVENTARIO TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimientosInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MovimientosInventario] (
        [MovimientoId] int IDENTITY(1,1) NOT NULL,
        [InventarioId] int NOT NULL,
        [InstitucionID] int NOT NULL,
        
        -- Movement Details
        [TipoMovimiento] nvarchar(20) NOT NULL,
        [CantidadAnterior] int NOT NULL,
        [CantidadNueva] int NOT NULL,
        [CantidadCambiada] int NOT NULL,
        [Motivo] nvarchar(500) NULL,
        [NumeroDocumento] nvarchar(100) NULL,
        
        -- Transfer Details (if applicable)
        [TransferenciaId] int NULL,
        [TipoUbicacionOrigen] int NULL,
        [UbicacionIdOrigen] int NULL,
        [TipoUbicacionDestino] int NULL,
        [UbicacionIdDestino] int NULL,
        
        -- Audit Fields
        [FechaMovimiento] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [UsuarioId] nvarchar(450) NOT NULL,
        [DireccionIP] nvarchar(45) NULL,
        [Metadata] ntext NULL,
        
        -- Constraints
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY CLUSTERED ([MovimientoId] ASC),
        CONSTRAINT [FK_MovimientosInventario_InventarioUnificado] FOREIGN KEY([InventarioId]) 
            REFERENCES [dbo].[InventarioUnificado] ([InventarioId]),
        CONSTRAINT [FK_MovimientosInventario_Instituciones] FOREIGN KEY([InstitucionID]) 
            REFERENCES [dbo].[Instituciones] ([InstitucionID]),
        CONSTRAINT [FK_MovimientosInventario_AspNetUsers] FOREIGN KEY([UsuarioId]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [CK_MovimientosInventario_TipoMovimiento] CHECK ([TipoMovimiento] IN 
            ('Entrada', 'Salida', 'Transferencia', 'Ajuste', 'Consumo', 'Devolucion', 'Perdida', 'Sincronizacion'))
    )
    
    -- Indexes
    CREATE INDEX [IX_MovimientosInventario_InventarioId] ON [dbo].[MovimientosInventario]([InventarioId])
    CREATE INDEX [IX_MovimientosInventario_InstitucionID] ON [dbo].[MovimientosInventario]([InstitucionID])
    CREATE INDEX [IX_MovimientosInventario_FechaMovimiento] ON [dbo].[MovimientosInventario]([FechaMovimiento])
    CREATE INDEX [IX_MovimientosInventario_TipoMovimiento] ON [dbo].[MovimientosInventario]([TipoMovimiento])
    CREATE INDEX [IX_MovimientosInventario_UsuarioId] ON [dbo].[MovimientosInventario]([UsuarioId])
    
    PRINT 'Table MovimientosInventario created successfully'
END
ELSE
BEGIN
    PRINT 'Table MovimientosInventario already exists'
END
GO

-- =============================================
-- 2. ALERTAS DE INVENTARIO TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AlertasInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AlertasInventario] (
        [AlertaId] int IDENTITY(1,1) NOT NULL,
        [InventarioId] int NOT NULL,
        [InstitucionID] int NOT NULL,
        
        -- Alert Details
        [TipoAlerta] nvarchar(30) NOT NULL,
        [Severidad] nvarchar(20) NOT NULL,
        [Mensaje] nvarchar(500) NOT NULL,
        [CantidadActual] int NOT NULL,
        [UmbralConfigurado] int NULL,
        [EsActiva] bit NOT NULL DEFAULT 1,
        [FueReconocida] bit NOT NULL DEFAULT 0,
        
        -- Audit Fields
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [FechaReconocimiento] datetime2(7) NULL,
        [FechaResolucion] datetime2(7) NULL,
        [UsuarioReconocimiento] nvarchar(450) NULL,
        [UsuarioResolucion] nvarchar(450) NULL,
        [NotasResolucion] nvarchar(500) NULL,
        
        -- Constraints
        CONSTRAINT [PK_AlertasInventario] PRIMARY KEY CLUSTERED ([AlertaId] ASC),
        CONSTRAINT [FK_AlertasInventario_InventarioUnificado] FOREIGN KEY([InventarioId]) 
            REFERENCES [dbo].[InventarioUnificado] ([InventarioId]),
        CONSTRAINT [FK_AlertasInventario_Instituciones] FOREIGN KEY([InstitucionID]) 
            REFERENCES [dbo].[Instituciones] ([InstitucionID]),
        CONSTRAINT [FK_AlertasInventario_AspNetUsers_Reconocimiento] FOREIGN KEY([UsuarioReconocimiento]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_AlertasInventario_AspNetUsers_Resolucion] FOREIGN KEY([UsuarioResolucion]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [CK_AlertasInventario_TipoAlerta] CHECK ([TipoAlerta] IN 
            ('StockBajo', 'StockAlto', 'StockAgotado', 'StockCritico', 'ProximoVencimiento', 'ArticuloInactivo', 'DiscrepanciaInventario')),
        CONSTRAINT [CK_AlertasInventario_Severidad] CHECK ([Severidad] IN ('Baja', 'Media', 'Alta', 'Critica'))
    )
    
    -- Indexes
    CREATE INDEX [IX_AlertasInventario_InventarioId] ON [dbo].[AlertasInventario]([InventarioId])
    CREATE INDEX [IX_AlertasInventario_InstitucionID] ON [dbo].[AlertasInventario]([InstitucionID])
    CREATE INDEX [IX_AlertasInventario_EsActiva] ON [dbo].[AlertasInventario]([EsActiva])
    CREATE INDEX [IX_AlertasInventario_FueReconocida] ON [dbo].[AlertasInventario]([FueReconocida])
    CREATE INDEX [IX_AlertasInventario_TipoAlerta] ON [dbo].[AlertasInventario]([TipoAlerta])
    CREATE INDEX [IX_AlertasInventario_Severidad] ON [dbo].[AlertasInventario]([Severidad])
    CREATE INDEX [IX_AlertasInventario_FechaCreacion] ON [dbo].[AlertasInventario]([FechaCreacion])
    
    PRINT 'Table AlertasInventario created successfully'
END
ELSE
BEGIN
    PRINT 'Table AlertasInventario already exists'
END
GO

-- =============================================
-- 3. CONFIGURACION ALERTAS INVENTARIO TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionAlertasInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConfiguracionAlertasInventario] (
        [ConfiguracionId] int IDENTITY(1,1) NOT NULL,
        [InventarioId] int NOT NULL,
        [InstitucionID] int NOT NULL,
        
        -- Alert Configuration
        [UmbralStockBajo] int NULL,
        [UmbralStockAlto] int NULL,
        [UmbralStockCritico] int NULL,
        [AlertasStockBajoActivas] bit NOT NULL DEFAULT 1,
        [AlertasStockAltoActivas] bit NOT NULL DEFAULT 0,
        [AlertasStockCriticoActivas] bit NOT NULL DEFAULT 1,
        [NotificacionEmailActiva] bit NOT NULL DEFAULT 1,
        [NotificacionSmsActiva] bit NOT NULL DEFAULT 0,
        
        -- Audit Fields
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [FechaActualizacion] datetime2(7) NULL,
        [UsuarioCreacion] nvarchar(450) NOT NULL,
        [UsuarioActualizacion] nvarchar(450) NULL,
        
        -- Constraints
        CONSTRAINT [PK_ConfiguracionAlertasInventario] PRIMARY KEY CLUSTERED ([ConfiguracionId] ASC),
        CONSTRAINT [FK_ConfiguracionAlertasInventario_InventarioUnificado] FOREIGN KEY([InventarioId]) 
            REFERENCES [dbo].[InventarioUnificado] ([InventarioId]),
        CONSTRAINT [FK_ConfiguracionAlertasInventario_Instituciones] FOREIGN KEY([InstitucionID]) 
            REFERENCES [dbo].[Instituciones] ([InstitucionID]),
        CONSTRAINT [FK_ConfiguracionAlertasInventario_AspNetUsers_Creacion] FOREIGN KEY([UsuarioCreacion]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_ConfiguracionAlertasInventario_AspNetUsers_Actualizacion] FOREIGN KEY([UsuarioActualizacion]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [UQ_ConfiguracionAlertasInventario_InventarioId] UNIQUE ([InventarioId])
    )
    
    -- Indexes
    CREATE INDEX [IX_ConfiguracionAlertasInventario_InstitucionID] ON [dbo].[ConfiguracionAlertasInventario]([InstitucionID])
    CREATE INDEX [IX_ConfiguracionAlertasInventario_UsuarioCreacion] ON [dbo].[ConfiguracionAlertasInventario]([UsuarioCreacion])
    
    PRINT 'Table ConfiguracionAlertasInventario created successfully'
END
ELSE
BEGIN
    PRINT 'Table ConfiguracionAlertasInventario already exists'
END
GO

-- =============================================
-- 4. TRANSFERENCIAS INVENTARIO TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransferenciasInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TransferenciasInventario] (
        [TransferenciaId] int IDENTITY(1,1) NOT NULL,
        [InstitucionID] int NOT NULL,
        
        -- Transfer Details
        [NumeroTransferencia] nvarchar(50) NOT NULL,
        [TipoUbicacionOrigen] int NOT NULL,
        [UbicacionIdOrigen] int NULL,
        [TipoUbicacionDestino] int NOT NULL,
        [UbicacionIdDestino] int NULL,
        [Estado] nvarchar(20) NOT NULL DEFAULT 'Pendiente',
        [Prioridad] nvarchar(20) NOT NULL DEFAULT 'Media',
        [Motivo] nvarchar(500) NULL,
        [Notas] nvarchar(1000) NULL,
        [FechaEsperada] datetime2(7) NULL,
        
        -- Approval Workflow
        [RequiereAprobacion] bit NOT NULL DEFAULT 1,
        [UsuarioAprobacion] nvarchar(450) NULL,
        [FechaAprobacion] datetime2(7) NULL,
        [ComentariosAprobacion] nvarchar(500) NULL,
        [UsuarioRechazo] nvarchar(450) NULL,
        [FechaRechazo] datetime2(7) NULL,
        [MotivoRechazo] nvarchar(500) NULL,
        
        -- Completion Details
        [UsuarioCompletado] nvarchar(450) NULL,
        [FechaCompletado] datetime2(7) NULL,
        [NotasCompletado] nvarchar(500) NULL,
        
        -- Audit Fields
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [FechaActualizacion] datetime2(7) NULL,
        [UsuarioCreacion] nvarchar(450) NOT NULL,
        [UsuarioActualizacion] nvarchar(450) NULL,
        [DireccionIP] nvarchar(45) NULL,
        
        -- Constraints
        CONSTRAINT [PK_TransferenciasInventario] PRIMARY KEY CLUSTERED ([TransferenciaId] ASC),
        CONSTRAINT [FK_TransferenciasInventario_Instituciones] FOREIGN KEY([InstitucionID]) 
            REFERENCES [dbo].[Instituciones] ([InstitucionID]),
        CONSTRAINT [FK_TransferenciasInventario_AspNetUsers_Creacion] FOREIGN KEY([UsuarioCreacion]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_TransferenciasInventario_AspNetUsers_Aprobacion] FOREIGN KEY([UsuarioAprobacion]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_TransferenciasInventario_AspNetUsers_Completado] FOREIGN KEY([UsuarioCompletado]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [UQ_TransferenciasInventario_NumeroTransferencia] UNIQUE ([NumeroTransferencia]),
        CONSTRAINT [CK_TransferenciasInventario_Estado] CHECK ([Estado] IN 
            ('Pendiente', 'Aprobada', 'Rechazada', 'EnProceso', 'Completada', 'Cancelada', 'ParcialmenteCompletada')),
        CONSTRAINT [CK_TransferenciasInventario_Prioridad] CHECK ([Prioridad] IN ('Baja', 'Media', 'Alta', 'Urgente'))
    )
    
    -- Indexes
    CREATE INDEX [IX_TransferenciasInventario_InstitucionID] ON [dbo].[TransferenciasInventario]([InstitucionID])
    CREATE INDEX [IX_TransferenciasInventario_Estado] ON [dbo].[TransferenciasInventario]([Estado])
    CREATE INDEX [IX_TransferenciasInventario_Prioridad] ON [dbo].[TransferenciasInventario]([Prioridad])
    CREATE INDEX [IX_TransferenciasInventario_FechaCreacion] ON [dbo].[TransferenciasInventario]([FechaCreacion])
    CREATE INDEX [IX_TransferenciasInventario_UsuarioCreacion] ON [dbo].[TransferenciasInventario]([UsuarioCreacion])
    CREATE INDEX [IX_TransferenciasInventario_NumeroTransferencia] ON [dbo].[TransferenciasInventario]([NumeroTransferencia])
    
    PRINT 'Table TransferenciasInventario created successfully'
END
ELSE
BEGIN
    PRINT 'Table TransferenciasInventario already exists'
END
GO

-- =============================================
-- 5. DETALLES TRANSFERENCIA INVENTARIO TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetallesTransferenciaInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DetallesTransferenciaInventario] (
        [DetalleId] int IDENTITY(1,1) NOT NULL,
        [TransferenciaId] int NOT NULL,
        [InventarioId] int NOT NULL,
        [ArticuloId] int NOT NULL,
        
        -- Transfer Item Details
        [CantidadSolicitada] int NOT NULL,
        [CantidadTransferida] int NULL,
        [CantidadDisponible] int NULL,
        [Notas] nvarchar(500) NULL,
        [FueTransferido] bit NULL,
        [MotivoFallo] nvarchar(200) NULL,
        
        -- Constraints
        CONSTRAINT [PK_DetallesTransferenciaInventario] PRIMARY KEY CLUSTERED ([DetalleId] ASC),
        CONSTRAINT [FK_DetallesTransferenciaInventario_TransferenciasInventario] FOREIGN KEY([TransferenciaId]) 
            REFERENCES [dbo].[TransferenciasInventario] ([TransferenciaId]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesTransferenciaInventario_InventarioUnificado] FOREIGN KEY([InventarioId]) 
            REFERENCES [dbo].[InventarioUnificado] ([InventarioId]),
        CONSTRAINT [FK_DetallesTransferenciaInventario_Articulos] FOREIGN KEY([ArticuloId]) 
            REFERENCES [dbo].[Articulos] ([ArticuloId]),
        CONSTRAINT [CK_DetallesTransferenciaInventario_CantidadSolicitada] CHECK ([CantidadSolicitada] > 0),
        CONSTRAINT [CK_DetallesTransferenciaInventario_CantidadTransferida] CHECK ([CantidadTransferida] >= 0)
    )
    
    -- Indexes
    CREATE INDEX [IX_DetallesTransferenciaInventario_TransferenciaId] ON [dbo].[DetallesTransferenciaInventario]([TransferenciaId])
    CREATE INDEX [IX_DetallesTransferenciaInventario_InventarioId] ON [dbo].[DetallesTransferenciaInventario]([InventarioId])
    CREATE INDEX [IX_DetallesTransferenciaInventario_ArticuloId] ON [dbo].[DetallesTransferenciaInventario]([ArticuloId])
    CREATE INDEX [IX_DetallesTransferenciaInventario_FueTransferido] ON [dbo].[DetallesTransferenciaInventario]([FueTransferido])
    
    PRINT 'Table DetallesTransferenciaInventario created successfully'
END
ELSE
BEGIN
    PRINT 'Table DetallesTransferenciaInventario already exists'
END
GO

-- =============================================
-- 6. ADD MISSING COLUMNS TO EXISTING TABLES (IF NEEDED)
-- =============================================

-- Check if InventarioUnificado table needs additional alert-related columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InventarioUnificado') AND name = 'CantidadMinima')
BEGIN
    ALTER TABLE [dbo].[InventarioUnificado] ADD [CantidadMinima] int NULL DEFAULT 0
    PRINT 'Column CantidadMinima added to InventarioUnificado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InventarioUnificado') AND name = 'CantidadMaxima')
BEGIN
    ALTER TABLE [dbo].[InventarioUnificado] ADD [CantidadMaxima] int NULL
    PRINT 'Column CantidadMaxima added to InventarioUnificado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InventarioUnificado') AND name = 'PuntoReorden')
BEGIN
    ALTER TABLE [dbo].[InventarioUnificado] ADD [PuntoReorden] int NULL DEFAULT 0
    PRINT 'Column PuntoReorden added to InventarioUnificado'
END

-- =============================================
-- 7. CREATE VIEWS FOR EASY QUERYING
-- =============================================

-- View for inventory movements with related information
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_MovimientosInventarioDetallado')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_MovimientosInventarioDetallado] AS
    SELECT 
        m.MovimientoId,
        m.InventarioId,
        m.InstitucionID,
        m.TipoMovimiento,
        m.CantidadAnterior,
        m.CantidadNueva,
        m.CantidadCambiada,
        m.Motivo,
        m.NumeroDocumento,
        m.FechaMovimiento,
        m.UsuarioId,
        u.UserName as UsuarioNombre,
        i.ArticuloId,
        a.NombreArticulo as ArticuloNombre,
        a.Precio as ArticuloPrecio,
        i.TipoUbicacion,
        i.UbicacionId,
        CASE 
            WHEN i.TipoUbicacion = 1 AND h.NombreHabitacion IS NOT NULL 
            THEN ''Habitación '' + h.NombreHabitacion
            WHEN i.TipoUbicacion = 0 
            THEN ''Inventario General''
            ELSE ''Ubicación Desconocida''
        END as UbicacionNombre
    FROM MovimientosInventario m
    INNER JOIN InventarioUnificado i ON m.InventarioId = i.InventarioId
    INNER JOIN Articulos a ON i.ArticuloId = a.ArticuloId
    INNER JOIN AspNetUsers u ON m.UsuarioId = u.Id
    LEFT JOIN Habitaciones h ON i.TipoUbicacion = 1 AND i.UbicacionId = h.HabitacionId
    ')
    PRINT 'View vw_MovimientosInventarioDetallado created successfully'
END

-- View for active alerts with details
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_AlertasInventarioActivas')
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_AlertasInventarioActivas] AS
    SELECT 
        al.AlertaId,
        al.InventarioId,
        al.InstitucionID,
        al.TipoAlerta,
        al.Severidad,
        al.Mensaje,
        al.CantidadActual,
        al.UmbralConfigurado,
        al.FechaCreacion,
        al.FueReconocida,
        i.ArticuloId,
        a.NombreArticulo as ArticuloNombre,
        a.Precio as ArticuloPrecio,
        i.TipoUbicacion,
        i.UbicacionId,
        CASE 
            WHEN i.TipoUbicacion = 1 AND h.NombreHabitacion IS NOT NULL 
            THEN ''Habitación '' + h.NombreHabitacion
            WHEN i.TipoUbicacion = 0 
            THEN ''Inventario General''
            ELSE ''Ubicación Desconocida''
        END as UbicacionNombre
    FROM AlertasInventario al
    INNER JOIN InventarioUnificado i ON al.InventarioId = i.InventarioId
    INNER JOIN Articulos a ON i.ArticuloId = a.ArticuloId
    LEFT JOIN Habitaciones h ON i.TipoUbicacion = 1 AND i.UbicacionId = h.HabitacionId
    WHERE al.EsActiva = 1
    ')
    PRINT 'View vw_AlertasInventarioActivas created successfully'
END

-- =============================================
-- 8. CREATE STORED PROCEDURES FOR COMMON OPERATIONS
-- =============================================

-- Stored procedure to generate transfer number
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GenerarNumeroTransferencia')
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_GenerarNumeroTransferencia]
        @InstitucionID int
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @Numero nvarchar(50)
        DECLARE @Contador int
        DECLARE @Anio nvarchar(4) = CAST(YEAR(GETDATE()) AS nvarchar(4))
        DECLARE @Mes nvarchar(2) = RIGHT(''0'' + CAST(MONTH(GETDATE()) AS nvarchar(2)), 2)
        
        -- Get next sequential number for this institution, year, and month
        SELECT @Contador = ISNULL(MAX(CAST(RIGHT(NumeroTransferencia, 4) AS int)), 0) + 1
        FROM TransferenciasInventario 
        WHERE InstitucionID = @InstitucionID 
          AND NumeroTransferencia LIKE ''TR-'' + @Anio + @Mes + ''-%''
        
        SET @Numero = ''TR-'' + @Anio + @Mes + ''-'' + RIGHT(''0000'' + CAST(@Contador AS nvarchar(4)), 4)
        
        SELECT @Numero as NumeroTransferencia
    END
    ')
    PRINT 'Stored procedure sp_GenerarNumeroTransferencia created successfully'
END

-- =============================================
-- COMPLETION MESSAGE
-- =============================================
PRINT '=========================================='
PRINT 'Extended Inventory Tables Creation Complete!'
PRINT '=========================================='
PRINT 'Tables created:'
PRINT '- MovimientosInventario (Inventory movements tracking)'
PRINT '- AlertasInventario (Inventory alerts)'
PRINT '- ConfiguracionAlertasInventario (Alert configuration)'
PRINT '- TransferenciasInventario (Transfer requests)'
PRINT '- DetallesTransferenciaInventario (Transfer details)'
PRINT ''
PRINT 'Views created:'
PRINT '- vw_MovimientosInventarioDetallado'
PRINT '- vw_AlertasInventarioActivas'
PRINT ''
PRINT 'Stored procedures created:'
PRINT '- sp_GenerarNumeroTransferencia'
PRINT '=========================================='