-- Create Configuracion table for system configuration settings
-- This table stores configurable values that can be modified from the API

-- Create Configuracion table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Configuracion' AND xtype='U')
CREATE TABLE [Configuracion] (
    [ConfiguracionId] int IDENTITY(1,1) NOT NULL,
    [Clave] varchar(100) NOT NULL,
    [Valor] nvarchar(500) NOT NULL,
    [Descripcion] nvarchar(255) NULL,
    [Categoria] varchar(50) NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT GETDATE(),
    [FechaModificacion] datetime2 NULL,
    [Activo] bit NOT NULL DEFAULT 1,
    [InstitucionId] int NULL,
    CONSTRAINT [PK_Configuracion] PRIMARY KEY ([ConfiguracionId])
);

-- Create foreign key constraint to Institucion table if it exists
IF EXISTS (SELECT * FROM sysobjects WHERE name='Institucion' AND xtype='U')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Configuracion_Institucion')
    ALTER TABLE [Configuracion] ADD CONSTRAINT [FK_Configuracion_Institucion] 
    FOREIGN KEY ([InstitucionId]) REFERENCES [Institucion] ([InstitucionId]) ON DELETE SET NULL;
END

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Configuracion_Clave_InstitucionId')
CREATE UNIQUE INDEX [IX_Configuracion_Clave_InstitucionId] ON [Configuracion] ([Clave], [InstitucionId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Configuracion_Categoria')
CREATE INDEX [IX_Configuracion_Categoria] ON [Configuracion] ([Categoria]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Configuracion_Activo')
CREATE INDEX [IX_Configuracion_Activo] ON [Configuracion] ([Activo]);

PRINT 'Configuracion table created successfully'

-- Insert default configuration values
PRINT 'Inserting default configuration values...'

-- Default timer update interval (5 minutes)
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'TIMER_UPDATE_INTERVAL' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('TIMER_UPDATE_INTERVAL', '5', 'Default timer update interval in minutes', 'SYSTEM', NULL, 1);

-- Default session timeout (30 minutes)
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'SESSION_TIMEOUT' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('SESSION_TIMEOUT', '30', 'Default session timeout in minutes', 'SECURITY', NULL, 1);

-- Default page size for pagination
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'DEFAULT_PAGE_SIZE' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('DEFAULT_PAGE_SIZE', '25', 'Default number of items per page', 'UI', NULL, 1);

-- Maximum file upload size in MB
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'MAX_UPLOAD_SIZE_MB' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('MAX_UPLOAD_SIZE_MB', '10', 'Maximum file upload size in megabytes', 'SYSTEM', NULL, 1);

-- Default notification retention days
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'NOTIFICATION_RETENTION_DAYS' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('NOTIFICATION_RETENTION_DAYS', '30', 'Number of days to keep notifications', 'NOTIFICATIONS', NULL, 1);

-- Enable/disable automatic backups
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'AUTO_BACKUP_ENABLED' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('AUTO_BACKUP_ENABLED', 'true', 'Enable or disable automatic database backups', 'BACKUP', NULL, 1);

-- Backup interval in hours
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'BACKUP_INTERVAL_HOURS' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('BACKUP_INTERVAL_HOURS', '24', 'Interval between automatic backups in hours', 'BACKUP', NULL, 1);

-- Default currency
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'DEFAULT_CURRENCY' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('DEFAULT_CURRENCY', 'DOP', 'Default currency code', 'FINANCIAL', NULL, 1);

-- Maintenance mode
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'MAINTENANCE_MODE' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('MAINTENANCE_MODE', 'false', 'Enable or disable maintenance mode', 'SYSTEM', NULL, 1);

-- Default timezone
IF NOT EXISTS (SELECT 1 FROM Configuracion WHERE Clave = 'DEFAULT_TIMEZONE' AND InstitucionId IS NULL)
INSERT INTO Configuracion (Clave, Valor, Descripcion, Categoria, InstitucionId, Activo)
VALUES ('DEFAULT_TIMEZONE', 'America/Santo_Domingo', 'Default timezone for the application', 'SYSTEM', NULL, 1);

PRINT 'Default configuration values inserted successfully'

-- Summary
DECLARE @ConfigCount INT;
SELECT @ConfigCount = COUNT(*) FROM Configuracion WHERE Activo = 1;

PRINT 'Configuration Setup Summary:'
PRINT '- Total active configurations: ' + CAST(@ConfigCount AS NVARCHAR(10))
PRINT '- Timer update interval can now be configured via API endpoint:'
PRINT '  GET /api/v1/configuration/timer-update-interval'
PRINT '  PUT /api/v1/configuration/timer-update-interval'

PRINT 'Configuracion table setup completed successfully!'