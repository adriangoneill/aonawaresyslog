/*
Installer
Copyright (C)2007 Adrian O' Neill

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.IO;

using Aonaware.SyslogShared;
using Aonaware.Utility.Database;
using Aonaware.Utility.Networking;

namespace Aonaware.SyslogWeb
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : SyslogInstaller
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            try
            {
                if (SkipDatabaseInstallation)
                {
                    EventLog.WriteEntry(_syslogWebName,
                        "Installation of database skipped - manual configuration will be required",
                        EventLogEntryType.Warning);
                    return;
                }

                DatabaseInstaller dbInsSys = GetSystemInstaller();
                DatabaseInstaller dbInsUser = GetUserInstaller();

                // Check that the system login can access the database
                try
                {
                    dbInsSys.TestMasterConnection();
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(
                        "Unable to connect to the database server as user {0}: {1}",
                        dbInsUser.UserName, ex.Message));
                }

                // If user login given, check that too
                if (!dbInsUser.TrustedConnection)
                {
                    try
                    {
                        dbInsUser.TestMasterConnection();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format(
                            "Unable to connect to the database server as user {0}: {1}",
                            dbInsUser.UserName, ex.Message));
                    }
                }

                // Check that the database already exists
                if (!dbInsSys.DatabaseExists)
                {
                    throw new InstallException(
                        String.Format("The database {0} does not exists on the server {1}",
                        dbInsSys.Database, dbInsSys.Server));
                }

                // If we are installing this application
                //  - on the same computer as SQL Server / MSDE
                //  - using IWA
                // Then add the ASP.NET service account to the database
                if (dbInsUser.TrustedConnection && dbInsUser.LocalServer)
                {
                    try
                    {
                        // Determine which account to use
                        string accountDBName = String.Empty;
                        string accountNTName = String.Empty;

                        // Search for IIS user

                        List<LocalUsersAndGroups.User> au = LocalUsersAndGroups.Users();
                        foreach (LocalUsersAndGroups.User u in au)
                        {
                            if (u.Name.ToUpper() == _aspNet)
                            {
                                accountDBName = u.Name;
                                accountNTName = u.Caption;
                                break;
                            }
                        }

                        if (accountDBName.Length == 0)
                        {
                            // Assume network service
                            accountDBName = "NETWORK SERVICE";
                            accountNTName = @"NT AUTHORITY\NETWORK SERVICE";
                        }

                        // Add account to database if needed
                        Dictionary<string, string> rep = new Dictionary<string, string>();
                        rep["%NTUSER%"] = accountNTName;
                        rep["%NEWDBUSER%"] = accountDBName;

                        dbInsSys.ExecuteGenericSQL(Assembly.GetExecutingAssembly(),
                            "Aonaware.SyslogWeb.database.createuser.sql", rep);

                        // Add some info to the event log
                        EventLog.WriteEntry(_syslogWebName, String.Format(
                            "Automatically added account {0} to database", accountNTName),
                            EventLogEntryType.Information);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(_syslogWebName,
                            String.Format("Unable to create ASP.NET account on database server: {0}",
                            ex.Message), EventLogEntryType.Warning);
                    }
                }

                // If we are using SQL Server authentication, give access to database
                if (!dbInsUser.TrustedConnection)
                {
                    Dictionary<string, string> rep = new Dictionary<string, string>();
                    rep["%SVCUSER%"] = dbInsUser.UserName;

                    dbInsSys.ExecuteGenericSQL(Assembly.GetExecutingAssembly(),
                        "Aonaware.SyslogWeb.database.users.sql", rep);
                }

                // Finally modify the config file
                string targetDir = TargetDirectory;
                string configFile = Path.Combine(targetDir, "Web.config");
                dbInsUser.ModifyConfigFile(configFile, "Aonaware.SyslogWeb.Properties.Settings.DbConnection");
            }
            catch (InstallException inst)
            {
                throw inst;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_syslogWebName, ex.Message,
                    EventLogEntryType.Error);
                throw new InstallException(ex.Message);
            }
        }

        private const string _syslogWebName = "Syslog Web Client";
        private const string _aspNet = "ASPNET";
    }
}