BEGIN TRANSACTION;
GO

ALTER TABLE [Imagenes] ADD [Origen] nvarchar(500) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250701182811_AddMissingImageFields', N'7.0.0');
GO

COMMIT;
GO

