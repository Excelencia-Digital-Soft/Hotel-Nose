-- Fix Visitas table to add missing HabitacionId column
-- This will align the database schema with the Entity Framework model

USE [Hotel] -- Replace with your actual database name
GO

-- Step 1: Add HabitacionId column to Visitas table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Visitas]') 
               AND name = 'HabitacionId')
BEGIN
    PRINT 'Adding HabitacionId column to Visitas table...'
    
    ALTER TABLE [dbo].[Visitas]
    ADD [HabitacionId] [int] NULL;
    
    PRINT 'HabitacionId column added successfully.'
END
ELSE
BEGIN
    PRINT 'HabitacionId column already exists in Visitas table.'
END
GO

-- Step 2: Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE object_id = OBJECT_ID(N'[dbo].[FK_Visitas_Habitaciones_HabitacionId]'))
BEGIN
    PRINT 'Adding foreign key constraint FK_Visitas_Habitaciones_HabitacionId...'
    
    ALTER TABLE [dbo].[Visitas]
    ADD CONSTRAINT [FK_Visitas_Habitaciones_HabitacionId] 
    FOREIGN KEY ([HabitacionId]) 
    REFERENCES [dbo].[Habitaciones] ([HabitacionID])
    ON DELETE SET NULL;
    
    PRINT 'Foreign key constraint added successfully.'
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_Visitas_Habitaciones_HabitacionId already exists.'
END
GO

-- Step 3: Add index for performance if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE object_id = OBJECT_ID(N'[dbo].[Visitas]') 
               AND name = 'IX_Visitas_HabitacionId')
BEGIN
    PRINT 'Adding index IX_Visitas_HabitacionId...'
    
    CREATE NONCLUSTERED INDEX [IX_Visitas_HabitacionId] 
    ON [dbo].[Visitas] ([HabitacionId] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, 
          SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, 
          ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
    ON [PRIMARY];
    
    PRINT 'Index IX_Visitas_HabitacionId added successfully.'
END
ELSE
BEGIN
    PRINT 'Index IX_Visitas_HabitacionId already exists.'
END
GO

-- Step 4: Verify the changes
PRINT 'Verification of changes:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Visitas' 
AND COLUMN_NAME IN ('VisitaID', 'HabitacionId', 'InstitucionID')
ORDER BY ORDINAL_POSITION;

PRINT 'Script execution completed successfully!'
GO