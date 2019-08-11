-- database creation script

-- drop everything

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_syslog_facilityCode]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE [dbo].[syslog] DROP CONSTRAINT FK_syslog_facilityCode
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_syslog_severityCode]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE [dbo].[syslog] DROP CONSTRAINT FK_syslog_severityCode
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SyslogPagingResults]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[SyslogPagingResults]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[viewSyslogDesc]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[viewSyslogDesc]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[configuration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[configuration]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[facilityCode]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[facilityCode]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[severityCode]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[severityCode]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[syslog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[syslog]
GO

-- Add roles
if not exists (select * from dbo.sysusers where name = N'syslog_iis' and uid > 16399)
	EXEC sp_addrole N'syslog_iis'
GO

if not exists (select * from dbo.sysusers where name = N'syslog_svc' and uid > 16399)
	EXEC sp_addrole N'syslog_svc'
GO


-- configuration table
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

-- facility code table
CREATE TABLE [dbo].[facilityCode] (
	[id] [tinyint] NOT NULL ,
	[description] [char] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[facilityCode] WITH NOCHECK ADD 
	CONSTRAINT [PK_facilityCode] PRIMARY KEY  CLUSTERED 
	(
		[id]
	)  ON [PRIMARY] 
GO

-- severity code table
CREATE TABLE [dbo].[severityCode] (
	[id] [tinyint] NOT NULL ,
	[description] [char] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[severityCode] WITH NOCHECK ADD 
	CONSTRAINT [PK_severityCode] PRIMARY KEY  CLUSTERED 
	(
		[id]
	)  ON [PRIMARY] 
GO

-- main syslog table
CREATE TABLE [dbo].[syslog] (
	[id] [bigint] IDENTITY (1, 1) NOT NULL ,
	[receivedTime] [datetime] NOT NULL ,
	[address] [char] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[localTime] [datetime] NOT NULL ,
	[facility] [tinyint] NOT NULL ,
	[severity] [tinyint] NOT NULL ,
	[message] [varchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[syslog] WITH NOCHECK ADD 
	CONSTRAINT [PK_syslog] PRIMARY KEY  CLUSTERED 
	(
		[id]
	)  ON [PRIMARY] 
GO


CREATE  INDEX [IX_syslog_address] ON [dbo].[syslog]([address]) ON [PRIMARY]
GO

CREATE  INDEX [IX_syslog_facility] ON [dbo].[syslog]([facility]) ON [PRIMARY]
GO

CREATE  INDEX [IX_syslog_severity] ON [dbo].[syslog]([severity]) ON [PRIMARY]
GO

-- viewSyslogDesc

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE VIEW viewSyslogDesc
AS
SELECT s.id, s.receivedTime, s.address, s.localTime,
	fc.description AS facilityCode, sc.description AS severityCode, s.message
FROM syslog s
INNER JOIN facilityCode fc ON facility = fc.id
INNER JOIN severityCode sc ON severity = sc.id

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

-- SyslogPagingResults stored proc

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE SyslogPagingResults
(
@pageIndex int,
@pageCount int,
@lastIndex bigint = -1)

AS

IF @lastIndex <= 0
BEGIN
	SELECT @lastIndex = MAX(id) FROM viewSyslogDesc
END

DECLARE @total bigint
SELECT @total = count(id) FROM viewSyslogDesc WHERE id <= @lastIndex

DECLARE @offset bigint
SELECT @offset = @pageCount * @pageIndex + 1

if (@offset > @total)
BEGIN
	DECLARE @remain bigint
	SELECT @remain = @pageCount - (@offset - @total) + 1
	SET rowcount @remain
	SELECT * FROM viewSyslogDesc 
		WHERE id <= @lastIndex
		ORDER BY id
END
else
BEGIN
	DECLARE @sid bigint
	SET rowcount @offset
	SELECT @sid = id FROM viewSyslogDesc
		WHERE id <= @lastIndex
		ORDER BY id DESC

	SET rowcount  @pageCount
	SELECT * FROM viewSyslogDesc
		WHERE id > @sid
		AND id <= @lastIndex
		ORDER BY id
END


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

-- table, view and sp permissions
GRANT  SELECT  ON [dbo].[configuration]  TO [syslog_iis]
GO

GRANT  SELECT ,  UPDATE ,  INSERT ,  DELETE  ON [dbo].[configuration]  TO [syslog_svc]
GO

GRANT  SELECT  ON [dbo].[facilityCode]  TO [syslog_iis]
GO

GRANT  SELECT  ON [dbo].[facilityCode]  TO [syslog_svc]
GO

GRANT  SELECT  ON [dbo].[severityCode]  TO [syslog_iis]
GO

GRANT  SELECT  ON [dbo].[severityCode]  TO [syslog_svc]
GO

GRANT  SELECT  ON [dbo].[syslog]  TO [syslog_iis]
GO

GRANT  SELECT ,  INSERT ,  DELETE  ON [dbo].[syslog]  TO [syslog_svc]
GO

GRANT  SELECT  ON [dbo].[viewSyslogDesc]  TO [syslog_iis]
GO

GRANT  SELECT  ON [dbo].[viewSyslogDesc]  TO [syslog_svc]
GO

GRANT  EXECUTE  ON [dbo].[SyslogPagingResults]  TO [syslog_iis]
GO

GRANT  EXECUTE  ON [dbo].[SyslogPagingResults]  TO [syslog_svc]
GO

-- foreign key relationship
ALTER TABLE [dbo].[syslog] ADD 
	CONSTRAINT [FK_syslog_facilityCode] FOREIGN KEY 
	(
		[facility]
	) REFERENCES [dbo].[facilityCode] (
		[id]
	),
	CONSTRAINT [FK_syslog_severityCode] FOREIGN KEY 
	(
		[severity]
	) REFERENCES [dbo].[severityCode] (
		[id]
	)
GO

-- data

DELETE FROM severityCode
insert severityCode select 0, 'Emergency'
insert severityCode select 1, 'Alert'
insert severityCode select 2, 'Critical'
insert severityCode select 3, 'Error'
insert severityCode select 4, 'Warning'
insert severityCode select 5, 'Notice'
insert severityCode select 6, 'Informational'
insert severityCode select 7, 'Debug'
GO

DELETE FROM facilityCode
insert facilityCode select 0, 'Kernel'                                                                                                                          
insert facilityCode select 1, 'User Level'                                                                                                                      
insert facilityCode select 2, 'Mail System'                                                                                                                     
insert facilityCode select 3, 'System Daemon'                                                                                                                   
insert facilityCode select 4, 'Security / Authorization (1)'                                                                                                    
insert facilityCode select 5, 'Syslogd Internal'                                                                                                                
insert facilityCode select 6, 'Printer'                                                                                                                         
insert facilityCode select 7, 'Network News'                                                                                                                    
insert facilityCode select 8, 'UUCP'                                                                                                                            
insert facilityCode select 9, 'Clock Daemon (1)'                                                                                                                
insert facilityCode select 10, 'Security / Authorization (2)'                                                                                                    
insert facilityCode select 11, 'FTP'                                                                                                                            
insert facilityCode select 12, 'NTP'                                                                                                                             
insert facilityCode select 13, 'Log Audit'                                                                                                                       
insert facilityCode select 14, 'Log Alert'                                                                                                                       
insert facilityCode select 15, 'Clock Daemon (2)'                                                                                                                
insert facilityCode select 16, 'Local Use 0'                                                                                                                     
insert facilityCode select 17, 'Local Use 1'                                                                                                                     
insert facilityCode select 18, 'Local Use 2'                                                                                                                     
insert facilityCode select 19, 'Local Use 3'                                                                                                                     
insert facilityCode select 20, 'Local Use 4'                                                                                                                     
insert facilityCode select 21, 'Local Use 5'                                                                                                                     
insert facilityCode select 22, 'Local Use 6'                                                                                                                     
insert facilityCode select 23, 'Local Use 7'                                                                                                                     
GO
