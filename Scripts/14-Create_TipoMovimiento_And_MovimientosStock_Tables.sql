-- Script 14: Create TipoMovimiento and MovimientosStock tables
-- These tables track stock movement types and movements for inventory management
-- Date: 2025-07-22

-- First, create TipoMovimiento table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TipoMovimiento]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TipoMovimiento] (
        [TipoMovimientoId] int IDENTITY(1,1) NOT NULL,
        [NombreTipoMovimiento] nvarchar(100) NULL,
        [Tipo] nvarchar(50) NULL,
        [Anulado] bit NULL DEFAULT 0,
        
        CONSTRAINT [PK_TipoMovimiento] PRIMARY KEY CLUSTERED ([TipoMovimientoId] ASC)
    );

    -- Insert default movement types
    SET IDENTITY_INSERT [dbo].[TipoMovimiento] ON;
    INSERT INTO [dbo].[TipoMovimiento] ([TipoMovimientoId], [NombreTipoMovimiento], [Tipo], [Anulado]) 
    VALUES 
        (1, 'Ingreso de Stock', 'INGRESO', 0),
        (2, 'Egreso de Stock', 'EGRESO', 0);
    SET IDENTITY_INSERT [dbo].[TipoMovimiento] OFF;

    PRINT 'TipoMovimiento table created successfully with default data';
END
ELSE
BEGIN
    PRINT 'TipoMovimiento table already exists';
    
    -- Ensure default movement types exist
    IF NOT EXISTS (SELECT * FROM TipoMovimiento WHERE TipoMovimientoId = 1)
    BEGIN
        SET IDENTITY_INSERT [dbo].[TipoMovimiento] ON;
        INSERT INTO TipoMovimiento ([TipoMovimientoId], [NombreTipoMovimiento], [Tipo], [Anulado]) 
        VALUES (1, 'Ingreso de Stock', 'INGRESO', 0);
        SET IDENTITY_INSERT [dbo].[TipoMovimiento] OFF;
        PRINT 'Added TipoMovimiento: Ingreso';
    END

    IF NOT EXISTS (SELECT * FROM TipoMovimiento WHERE TipoMovimientoId = 2)
    BEGIN
        SET IDENTITY_INSERT [dbo].[TipoMovimiento] ON;
        INSERT INTO TipoMovimiento ([TipoMovimientoId], [NombreTipoMovimiento], [Tipo], [Anulado]) 
        VALUES (2, 'Egreso de Stock', 'EGRESO', 0);
        SET IDENTITY_INSERT [dbo].[TipoMovimiento] OFF;
        PRINT 'Added TipoMovimiento: Egreso';
    END
END

-- Now create MovimientosStock table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimientosStock]') AND type in (N'U'))
BEGIN
    -- Create MovimientosStock table
    CREATE TABLE [dbo].[MovimientosStock] (
        [MovimientoId] int IDENTITY(1,1) NOT NULL,
        [ArticuloId] int NULL,
        [TipoMovimientoId] int NULL,
        [Cantidad] int NULL,
        [FechaMovimiento] datetime2(7) NULL,
        [MovimientosId] int NULL,
        [UsuarioId] int NULL,
        [FechaRegistro] datetime2(7) NULL,
        [Anulado] bit NULL DEFAULT 0,
        
        CONSTRAINT [PK_MovimientosStock] PRIMARY KEY CLUSTERED ([MovimientoId] ASC),
        
        -- Foreign key constraints
        CONSTRAINT [FK_MovimientosStock_Articulos] 
            FOREIGN KEY([ArticuloId]) REFERENCES [dbo].[Articulos] ([ArticuloId]),
        CONSTRAINT [FK_MovimientosStock_TipoMovimiento] 
            FOREIGN KEY([TipoMovimientoId]) REFERENCES [dbo].[TipoMovimiento] ([TipoMovimientoId]),
        CONSTRAINT [FK_MovimientosStock_Movimientos] 
            FOREIGN KEY([MovimientosId]) REFERENCES [dbo].[Movimientos] ([MovimientosId])
    );

    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX [IX_MovimientosStock_ArticuloId] ON [dbo].[MovimientosStock] ([ArticuloId]);
    CREATE NONCLUSTERED INDEX [IX_MovimientosStock_FechaMovimiento] ON [dbo].[MovimientosStock] ([FechaMovimiento]);
    CREATE NONCLUSTERED INDEX [IX_MovimientosStock_TipoMovimientoId] ON [dbo].[MovimientosStock] ([TipoMovimientoId]);

    PRINT 'MovimientosStock table created successfully with indexes';
END
ELSE
BEGIN
    PRINT 'MovimientosStock table already exists';
END

PRINT 'Script 14 completed successfully';