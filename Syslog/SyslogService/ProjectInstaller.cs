/*
Project Installer
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Diagnostics;
using System.Reflection;

using Aonaware.SyslogShared;
using Aonaware.Utility.Database;

namespace Aonaware.SyslogService 
{
	/// <summary>
	/// Installer for the Syslog Server
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : SyslogInstaller
	{
		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller serviceInstaller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();
		}

		protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
		{
			base.OnBeforeInstall(savedState);

			// Set up SQL server dependency (if required)
			if (SkipDatabaseInstallation)
				return;

			try
			{
				DatabaseInstaller dbInsSys = GetSystemInstaller();
				if (dbInsSys.LocalServer)
					serviceInstaller.ServicesDependedOn = new string[]
						{ dbInsSys.SQLServerServiceInstance() };
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Service.SyslogServiceName, ex.Message,
					EventLogEntryType.Error);
			}
		}

        public override void Install(System.Collections.IDictionary stateSaver)
		{
			base.Install(stateSaver);

			try
			{
				if (SkipDatabaseInstallation)
				{
					EventLog.WriteEntry(Service.SyslogServiceName,
						"Installation of database skipped - manual configuration will be required",
						EventLogEntryType.Warning);
					return;
				}

				DatabaseInstaller dbInsSys = GetSystemInstaller();
				DatabaseInstaller dbInsUser = GetUserInstaller();

				// Ensure same username not given if not using IWA
				if (!dbInsSys.TrustedConnection && !dbInsUser.TrustedConnection
					&& (dbInsSys.UserName == dbInsUser.UserName))
				{
					throw new Exception("When not using Windows authentication the username given to "
						+ "create the database must be different from the username Syslog Server will use");
				}

				// Check database does not already exist 
				if (dbInsSys.DatabaseExists)
				{
					throw new InstallException(
						String.Format("The database {0} already exists on the server {1}",
						dbInsSys.Database, dbInsSys.Server));
				}

				// Check user can access the database server
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

				// Time to do the actual work

				// Create database
				dbInsSys.CreateDatabase(Assembly.GetExecutingAssembly(),
					"Aonaware.SyslogService.database.create.sql");

				// Add system logon credentials stateServer for uninstall / rollback
				// Todo: encrypt this
				stateSaver.Add(_serverString, dbInsSys.Server);
				stateSaver.Add(_dataString, dbInsSys.Database);
				stateSaver.Add(_userString, dbInsSys.UserName);
				stateSaver.Add(_passString, dbInsSys.Password);

				// If not using IWA for our user connection, add the user to the service role
				if (!dbInsUser.TrustedConnection)
				{
                    Dictionary<string, string> rep = new Dictionary<string, string>();
					rep["%SVCUSER%"] = dbInsUser.UserName;

					dbInsSys.ExecuteGenericSQL(Assembly.GetExecutingAssembly(),
						"Aonaware.SyslogService.database.users.sql", rep);
				}

				// Finally modify the config file
				string configFile = Assembly.GetExecutingAssembly().Location + ".config";
				dbInsUser.ModifyConfigFile(configFile, "Aonaware.SyslogService.Properties.Settings.DbConnection");
			}
			catch (InstallException inst)
			{
				throw inst;
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Service.SyslogServiceName, ex.Message,
					EventLogEntryType.Error);
				throw new InstallException(ex.Message);
			}
		}

        private void DropDatabase(System.Collections.IDictionary savedState)
		{
			// Try and drop the database
			try
			{
				if ((savedState[_serverString] == null) 
					|| (savedState[_dataString] == null)
					|| (savedState[_userString] == null)
					|| (savedState[_passString] == null))
				{
					// Had not completed database configuration - can safely ignore
					return;
				}

				DatabaseInstaller dbIns = new
					DatabaseInstaller((string) savedState[_serverString],
					(string) savedState[_dataString], 
					(string) savedState[_userString], 
					(string) savedState[_passString]);
				dbIns.DropDatabase();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Service.SyslogServiceName, ex.Message,
					EventLogEntryType.Error);
				// Not fatal - manual database uninstall required however
			}
		}

        public override void Uninstall(System.Collections.IDictionary savedState)
		{
			base.Uninstall(savedState);

			DropDatabase(savedState);
		}

        public override void Rollback(System.Collections.IDictionary savedState)
		{
			base.Rollback(savedState);

			DropDatabase(savedState);
		}

        protected override void OnCommitted(System.Collections.IDictionary savedState)
		{
			base.OnCommitted(savedState);

			// Try and start the service
			try
			{
				ServiceController me = new ServiceController(Service.SyslogServiceName); 
				if (me != null)
					me.Start();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(Service.SyslogServiceName,
					String.Format("Could not start service: {0}",
					ex.Message), EventLogEntryType.Error);
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller
			// 
			this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.serviceProcessInstaller.Password = null;
			this.serviceProcessInstaller.Username = null;
			// 
			// serviceInstaller
			// 
			this.serviceInstaller.DisplayName = "Aonaware Syslog Daemon";
			this.serviceInstaller.ServiceName = "Aonaware Syslog Daemon";
			this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.serviceProcessInstaller,
																					  this.serviceInstaller});

		}
		#endregion
	}
}
