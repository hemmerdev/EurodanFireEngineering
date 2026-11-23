USE EurodanDB;
GO

-- Check if the PROJECT table exists and if the IsArchived column does not exist
IF OBJECT_ID('dbo.PROJECT', 'U') IS NOT NULL
	AND COL_LENGTH('dbo.PROJECT', 'IsArchived') IS NULL
BEGIN
	ALTER TABLE dbo.PROJECT
	ADD IsArchived BIT NOT NULL
		CONSTRAINT DF_PROJECT_IsArchived DEFAULT 0;
END;


