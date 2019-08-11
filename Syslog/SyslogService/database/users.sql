-- Adds the user to the correct role
if not exists (select * from dbo.sysusers where name = N'%SVCUSER%' and uid < 16382)
	EXEC sp_grantdbaccess N'%SVCUSER%', N'%SVCUSER%'
GO

EXEC sp_addrolemember N'syslog_svc', N'%SVCUSER%'
GO
