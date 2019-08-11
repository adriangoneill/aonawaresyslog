-- Users table

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[users]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[users]
GO

CREATE TABLE [dbo].[users] (
	[userName] [char] (16) COLLATE Latin1_General_CI_AS NOT NULL ,
	[password] [char] (32) COLLATE Latin1_General_CI_AS NOT NULL ,
	[active] [bit] NOT NULL 
) ON [PRIMARY]
GO

-- Event logging

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[eventLog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[eventLog]
GO

CREATE TABLE [dbo].[eventLog] (
	[eventTime] [datetime] NOT NULL ,
	[system] [char] (16) COLLATE Latin1_General_CI_AS NOT NULL ,
	[category] [char] (16) COLLATE Latin1_General_CI_AS NOT NULL ,
	[message] [varchar] (512) COLLATE Latin1_General_CI_AS NOT NULL 
) ON [PRIMARY]
GO

CREATE  CLUSTERED  INDEX [IX_eventLog] ON [dbo].[eventLog]([eventTime]) ON [PRIMARY]
GO


-- Sample logging table
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sampleLog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[sampleLog]
GO

CREATE TABLE [dbo].[sampleLog] (
	[logTime] [datetime] NOT NULL ,
	[event] [char] (32) COLLATE Latin1_General_CI_AS NOT NULL
) ON [PRIMARY]
GO

