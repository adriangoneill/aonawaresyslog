-- configuration table

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[configuration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[configuration]
GO

CREATE TABLE [dbo].[configuration] (
	[id] [int] IDENTITY (1, 1) NOT NULL ,
	[system] [char] (16) COLLATE Latin1_General_CI_AS NOT NULL ,
	[attribute] [char] (256) COLLATE Latin1_General_CI_AS NOT NULL ,
	[type] [char] (64) COLLATE Latin1_General_CI_AS NOT NULL ,
	[val] [varchar] (1024) COLLATE Latin1_General_CI_AS NOT NULL 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[configuration] WITH NOCHECK ADD 
	CONSTRAINT [PK_configuration] PRIMARY KEY  CLUSTERED 
	(
		[id]
	)  ON [PRIMARY] 
GO

CREATE  UNIQUE  INDEX [IX_configuration] ON [dbo].[configuration]([system], [attribute]) ON [PRIMARY]
GO

