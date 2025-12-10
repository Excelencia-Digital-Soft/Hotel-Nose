IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Pagos]') AND name = 'InteresTarjeta')
BEGIN
    ALTER TABLE [dbo].[Pagos] ADD [InteresTarjeta] decimal(10, 2) NULL;
END
GO
