-- Script 14: Create Inventory Alerts Tables
-- Purpose: Create tables for inventory alert system
-- Date: 2025-08-01
-- Author: Claude AI Assistant

USE [HotelManagement]
GO

-- Create AlertasInventario table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AlertasInventario')
BEGIN
    CREATE TABLE [dbo].[AlertasInventario](
        [AlertaId] [int] IDENTITY(1,1) NOT NULL,
        [InventarioId] [int] NOT NULL,
        [InstitucionID] [int] NOT NULL,
        [TipoAlerta] [nvarchar](30) NOT NULL,
        [Severidad] [nvarchar](20) NOT NULL,
        [Mensaje] [nvarchar](500) NOT NULL,
        [CantidadActual] [int] NOT NULL,
        [UmbralConfiguracion] [int] NULL,
        [EsActiva] [bit] NOT NULL DEFAULT 1,
        [FueReconocida] [bit] NOT NULL DEFAULT 0,
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [FechaReconocimiento] [datetime2](7) NULL,
        [FechaResolucion] [datetime2](7) NULL,
        [UsuarioReconocimiento] [nvarchar](450) NULL,
        [UsuarioResolucion] [nvarchar](450) NULL,
        [NotasReconocimiento] [nvarchar](500) NULL,
        [NotasResolucion] [nvarchar](500) NULL,
        
        CONSTRAINT [PK_AlertasInventario] PRIMARY KEY CLUSTERED ([AlertaId] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    
    PRINT 'Table AlertasInventario created successfully.'
END
ELSE
BEGIN
    PRINT 'Table AlertasInventario already exists.'
END
GO

-- Create ConfiguracionAlertasInventario table  
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConfiguracionAlertasInventario')
BEGIN
    CREATE TABLE [dbo].[ConfiguracionAlertasInventario](
        [ConfiguracionId] [int] IDENTITY(1,1) NOT NULL,
        [InventarioId] [int] NOT NULL,
        [InstitucionID] [int] NOT NULL,
        [StockMinimo] [int] NULL,
        [StockMaximo] [int] NULL,
        [StockCritico] [int] NULL,
        [AlertasStockBajoActivas] [bit] NOT NULL DEFAULT 1,
        [AlertasStockAltoActivas] [bit] NOT NULL DEFAULT 0,
        [AlertasStockCriticoActivas] [bit] NOT NULL DEFAULT 1,
        [NotificacionEmailActiva] [bit] NOT NULL DEFAULT 1,
        [NotificacionSmsActiva] [bit] NOT NULL DEFAULT 0,
        [EmailsNotificacion] [nvarchar](1000) NULL,
        [TelefonosNotificacion] [nvarchar](500) NULL,
        [FrecuenciaRevisionMinutos] [int] NOT NULL DEFAULT 60,
        [EsActiva] [bit] NOT NULL DEFAULT 1,
        [FechaCreacion] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [FechaActualizacion] [datetime2](7) NULL,
        [UsuarioCreacion] [nvarchar](450) NOT NULL,
        [UsuarioActualizacion] [nvarchar](450) NULL,
        
        CONSTRAINT [PK_ConfiguracionAlertasInventario] PRIMARY KEY CLUSTERED ([ConfiguracionId] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    
    PRINT 'Table ConfiguracionAlertasInventario created successfully.'
END
ELSE
BEGIN
    PRINT 'Table ConfiguracionAlertasInventario already exists.'
END
GO

-- Add foreign key constraints for AlertasInventario
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AlertasInventario_InventarioUnificado')
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioUnificado')
    BEGIN
        ALTER TABLE [dbo].[AlertasInventario]
        ADD CONSTRAINT [FK_AlertasInventario_InventarioUnificado] 
        FOREIGN KEY([InventarioId]) REFERENCES [dbo].[InventarioUnificado] ([InventarioId])
        ON DELETE CASCADE
        
        PRINT 'Foreign key FK_AlertasInventario_InventarioUnificado created.'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: InventarioUnificado table does not exist. Foreign key not created.'
    END
END
ELSE
BEGIN
    PRINT 'Foreign key FK_AlertasInventario_InventarioUnificado already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AlertasInventario_Instituciones')
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Instituciones')
    BEGIN
        ALTER TABLE [dbo].[AlertasInventario]
        ADD CONSTRAINT [FK_AlertasInventario_Instituciones] 
        FOREIGN KEY([InstitucionID]) REFERENCES [dbo].[Instituciones] ([InstitucionID])
        ON DELETE NO ACTION
        
        PRINT 'Foreign key FK_AlertasInventario_Instituciones created.'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Instituciones table does not exist. Foreign key not created.'
    END
END
ELSE
BEGIN
    PRINT 'Foreign key FK_AlertasInventario_Instituciones already exists.'
END
GO

-- Add foreign key constraints for ConfiguracionAlertasInventario
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ConfiguracionAlertasInventario_InventarioUnificado')
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioUnificado')
    BEGIN
        ALTER TABLE [dbo].[ConfiguracionAlertasInventario]
        ADD CONSTRAINT [FK_ConfiguracionAlertasInventario_InventarioUnificado] 
        FOREIGN KEY([InventarioId]) REFERENCES [dbo].[InventarioUnificado] ([InventarioId])
        ON DELETE CASCADE
        
        PRINT 'Foreign key FK_ConfiguracionAlertasInventario_InventarioUnificado created.'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: InventarioUnificado table does not exist. Foreign key not created.'
    END
END
ELSE
BEGIN
    PRINT 'Foreign key FK_ConfiguracionAlertasInventario_InventarioUnificado already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ConfiguracionAlertasInventario_Instituciones')
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Instituciones')
    BEGIN
        ALTER TABLE [dbo].[ConfiguracionAlertasInventario]
        ADD CONSTRAINT [FK_ConfiguracionAlertasInventario_Instituciones] 
        FOREIGN KEY([InstitucionID]) REFERENCES [dbo].[Instituciones] ([InstitucionID])
        ON DELETE NO ACTION
        
        PRINT 'Foreign key FK_ConfiguracionAlertasInventario_Instituciones created.'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Instituciones table does not exist. Foreign key not created.'
    END
END
ELSE
BEGIN
    PRINT 'Foreign key FK_ConfiguracionAlertasInventario_Instituciones already exists.'
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AlertasInventario_InventarioId_EsActiva')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AlertasInventario_InventarioId_EsActiva]
    ON [dbo].[AlertasInventario] ([InventarioId] ASC, [EsActiva] ASC)
    INCLUDE ([TipoAlerta], [Severidad], [FechaCreacion])
    
    PRINT 'Index IX_AlertasInventario_InventarioId_EsActiva created.'
END
ELSE
BEGIN
    PRINT 'Index IX_AlertasInventario_InventarioId_EsActiva already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AlertasInventario_InstitucionID_FechaCreacion')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AlertasInventario_InstitucionID_FechaCreacion]
    ON [dbo].[AlertasInventario] ([InstitucionID] ASC, [FechaCreacion] DESC)
    INCLUDE ([TipoAlerta], [Severidad], [EsActiva])
    
    PRINT 'Index IX_AlertasInventario_InstitucionID_FechaCreacion created.'
END
ELSE
BEGIN
    PRINT 'Index IX_AlertasInventario_InstitucionID_FechaCreacion already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ConfiguracionAlertasInventario_InventarioId_EsActiva')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ConfiguracionAlertasInventario_InventarioId_EsActiva]
    ON [dbo].[ConfiguracionAlertasInventario] ([InventarioId] ASC, [EsActiva] ASC)
    INCLUDE ([StockMinimo], [StockMaximo], [StockCritico])
    
    PRINT 'Index IX_ConfiguracionAlertasInventario_InventarioId_EsActiva created.'
END
ELSE
BEGIN
    PRINT 'Index IX_ConfiguracionAlertasInventario_InventarioId_EsActiva already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ConfiguracionAlertasInventario_InstitucionID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ConfiguracionAlertasInventario_InstitucionID]
    ON [dbo].[ConfiguracionAlertasInventario] ([InstitucionID] ASC)
    INCLUDE ([EsActiva], [FechaCreacion])
    
    PRINT 'Index IX_ConfiguracionAlertasInventario_InstitucionID created.'
END
ELSE
BEGIN
    PRINT 'Index IX_ConfiguracionAlertasInventario_InstitucionID already exists.'
END
GO

-- Add unique constraint to prevent duplicate configurations per inventory item
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_ConfiguracionAlertasInventario_InventarioId')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionAlertasInventario]
    ADD CONSTRAINT [UQ_ConfiguracionAlertasInventario_InventarioId] 
    UNIQUE ([InventarioId])
    
    PRINT 'Unique constraint UQ_ConfiguracionAlertasInventario_InventarioId created.'
END
ELSE
BEGIN
    PRINT 'Unique constraint UQ_ConfiguracionAlertasInventario_InventarioId already exists.'
END
GO

-- Create check constraints for data validation
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AlertasInventario_TipoAlerta')
BEGIN
    ALTER TABLE [dbo].[AlertasInventario]
    ADD CONSTRAINT [CK_AlertasInventario_TipoAlerta] 
    CHECK ([TipoAlerta] IN ('StockBajo', 'StockAlto', 'StockAgotado', 'StockCritico', 'ProximoVencimiento', 'ArticuloInactivo', 'DiscrepanciaInventario'))
    
    PRINT 'Check constraint CK_AlertasInventario_TipoAlerta created.'
END
ELSE
BEGIN
    PRINT 'Check constraint CK_AlertasInventario_TipoAlerta already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AlertasInventario_Severidad')
BEGIN
    ALTER TABLE [dbo].[AlertasInventario]
    ADD CONSTRAINT [CK_AlertasInventario_Severidad] 
    CHECK ([Severidad] IN ('Baja', 'Media', 'Alta', 'Critica'))
    
    PRINT 'Check constraint CK_AlertasInventario_Severidad created.'
END
ELSE
BEGIN
    PRINT 'Check constraint CK_AlertasInventario_Severidad already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ConfiguracionAlertasInventario_StockValues')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionAlertasInventario]
    ADD CONSTRAINT [CK_ConfiguracionAlertasInventario_StockValues] 
    CHECK ([StockMinimo] >= 0 AND [StockMaximo] >= 0 AND [StockCritico] >= 0)
    
    PRINT 'Check constraint CK_ConfiguracionAlertasInventario_StockValues created.'
END
ELSE
BEGIN
    PRINT 'Check constraint CK_ConfiguracionAlertasInventario_StockValues already exists.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ConfiguracionAlertasInventario_FrecuenciaRevision')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionAlertasInventario]
    ADD CONSTRAINT [CK_ConfiguracionAlertasInventario_FrecuenciaRevision] 
    CHECK ([FrecuenciaRevisionMinutos] > 0 AND [FrecuenciaRevisionMinutos] <= 10080) -- Max 1 week
    
    PRINT 'Check constraint CK_ConfiguracionAlertasInventario_FrecuenciaRevision created.'
END
ELSE
BEGIN
    PRINT 'Check constraint CK_ConfiguracionAlertasInventario_FrecuenciaRevision already exists.'
END
GO

PRINT 'Script 14: Create Inventory Alerts Tables completed successfully!'
PRINT 'Tables created:'
PRINT '- AlertasInventario'
PRINT '- ConfiguracionAlertasInventario'
PRINT 'Foreign keys, indexes, and constraints created for optimal performance and data integrity.'

GO