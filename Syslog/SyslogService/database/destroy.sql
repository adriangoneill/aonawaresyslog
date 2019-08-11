-- Database destruction

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'%DATABASE%')
	DROP DATABASE [%DATABASE%]
GO

