-- Script to mark the InitialCreate migration as already applied
-- This tells Entity Framework that the existing database structure corresponds to this migration

-- Insert the migration record into __EFMigrationsHistory if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250701174952_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250701174952_InitialCreate', N'7.0.0');
    
    PRINT 'InitialCreate migration marked as applied';
END
ELSE
BEGIN
    PRINT 'InitialCreate migration already exists in history';
END

-- Now you can apply the AddMissingImageFields migration safely