-- Add Status column to Shops table
-- Run this on your database to add the new Status field

USE [SWP_Group6]
GO

-- Add Status column with default value of 0 (Pending)
ALTER TABLE [dbo].[Shops]
ADD [Status] INT NOT NULL DEFAULT 0;
GO

-- Update existing shops based on IsActive value
-- IsActive=1 (true) ? Status=1 (Active)
-- IsActive=0 (false) ? Status=0 (Pending)  
UPDATE [dbo].[Shops]
SET [Status] = 
    CASE 
        WHEN [isActive] = 1 THEN 1  -- Active
        ELSE 0  -- Pending (newly created)
    END;
GO

-- Add check constraint for Status values (0=Pending, 1=Active, 2=Suspended)
ALTER TABLE [dbo].[Shops]
ADD CONSTRAINT CHK_Shops_Status CHECK ([Status] IN (0, 1, 2));
GO

PRINT 'Migration completed successfully!';
PRINT 'Status column added with:';
PRINT '  0 = Pending (Ch? xác nh?n)';
PRINT '  1 = Active (Ho?t ð?ng)';
PRINT '  2 = Suspended (T?m d?ng)';
GO
