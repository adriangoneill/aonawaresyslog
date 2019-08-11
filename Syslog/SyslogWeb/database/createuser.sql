-- Create server login (if needed)
if not exists (select * from master.dbo.syslogins where loginname = N'%NTUSER%')
BEGIN
	exec sp_grantlogin N'%NTUSER%'
	exec sp_defaultdb N'%NTUSER%', N'master'
END
GO

-- Add to database / role
if not exists (select * from dbo.sysusers where name = N'%NEWDBUSER%' and uid < 16382)
	EXEC sp_grantdbaccess N'%NTUSER%', N'%NEWDBUSER%'
GO

EXEC sp_addrolemember N'syslog_iis', N'%NEWDBUSER%'
GO
