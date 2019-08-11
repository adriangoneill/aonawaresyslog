/*
SQL Server Database Installer
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
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Net;
using System.Management;

using Microsoft.Win32;

namespace Aonaware.Utility.Database
{
	/// <summary>
	/// Class used with setup project to install database
	/// </summary>
	public class DatabaseInstaller
	{
		public DatabaseInstaller(string server, string database,
			string userName, string password)
		{
			_server = server.Trim();
			_database = database.Trim();
			_userName = userName.Trim();
			_password = password.Trim();
		}

		public void TestMasterConnection()
		{
			string connString = BuildConnectionString("master");
			using (OleDbConnection conn = new OleDbConnection(connString))
			{
				conn.Open();
			}
		}

		public void TestConnection()
		{
			using (OleDbConnection conn = new OleDbConnection(ConnectionString))
			{
				conn.Open();
			}
		}

        public Version ServerVersion()
        {
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand("SELECT SERVERPROPERTY('productversion')", conn);
                string res = (string)cmd.ExecuteScalar();
                return new Version(res);
            }
        }

		public void ExecuteGenericSQL(Assembly a, string resourceScript)
		{
			ExecuteGenericSQL(a, resourceScript, null);
		}

		public void ExecuteGenericSQL(Assembly a, string resourceScript, 
            Dictionary<string, string> replacements)
		{
			// Default replacements
			if (replacements == null)
                replacements = new Dictionary<string, string>();

			replacements["%DATABASE%"] = Database;
			replacements["%USER%"] = UserName;

			using (Stream s = a.GetManifestResourceStream(resourceScript))
			{
				if (s == null)
				{
					throw new Exception("Unable to load SQL from resource file");
				}

				using (OleDbConnection conn = new OleDbConnection(ConnectionString))
				{
					conn.Open();

					StringBuilder sqlSb = new StringBuilder();
					using (StreamReader sr = new StreamReader(s))
					{
						String line;
						while ((line = sr.ReadLine()) != null)
						{
							// Replace any database strings
							string repsql = line.Trim();

							if (repsql.Length == 0)
								continue;

							// Do any replacements
							foreach (KeyValuePair<string, string> en in replacements)
                            {
								repsql = repsql.Replace(en.Key, en.Value);
							}
					
							if (repsql.ToUpper() == "GO")
							{
								// Execute
								using (OleDbCommand cmd = new OleDbCommand(sqlSb.ToString(), conn))
								{
									cmd.ExecuteNonQuery();
								}

								sqlSb.Length = 0;
							}
							else
							{
								sqlSb.Append(repsql);
								sqlSb.Append(Environment.NewLine);
							}
						}
					}
				}
			}
		}

		private void ExecuteMasterSQL(string sql)
		{
			using (OleDbConnection conn = new OleDbConnection(BuildConnectionString("master")))
			{
				conn.Open();
				using (OleDbCommand cmd = new OleDbCommand(sql, conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void CreateDatabase(Assembly a, string resourceScript)
		{
			// Ensure we can connect
			try
			{
				TestMasterConnection();
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("Unable to connect to server as user {0}: {1}",
					UserName, ex.Message));
			}

			// Ensure not already created
			if (DatabaseExists)
				throw new Exception(String.Format("The database {0} seems to have already been created",
					_database));

			// Roll back creation of the database from here on
			try
			{
				try
				{
					ExecuteMasterSQL(String.Format("CREATE DATABASE [{0}]", _database));
				}
				catch (Exception ex)
				{
					throw new Exception(String.Format("Unable to create database: {0}",
						ex.Message));
				}

				// Wait for the database to be created before logging in again.
				// No way around this that I can see.
				System.Threading.Thread.Sleep(5000);

				// Run SQL to create database items
				ExecuteGenericSQL(a, resourceScript);
			}
			catch (Exception ex)
			{
				try
				{
					DropDatabase();
				}
				finally
				{
					throw ex;
				}
			}
		}

		public void DropDatabase()
		{
			// Go nuclear
			string s =
				"IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}') " +
				"BEGIN " +
				"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE " +
				"DROP DATABASE [{0}] " +
				"END";
			string sql = String.Format(s, Database);
			try
			{
				ExecuteMasterSQL(sql);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("Unable to drop database: {0}",
					ex.Message));
			}
		}

		public void ModifyConfigFile(string configFile, string configName)
		{
			try
			{
				XmlDocument xdoc = new XmlDocument();
				xdoc.Load(configFile);
				XmlNode xnode = xdoc.SelectSingleNode(String.Format(
                    @"configuration/connectionStrings/add[@name='{0}']", configName));
				if (xnode == null)
					throw new Exception("Could not find correct node in config file");

                xnode.Attributes["connectionString"].Value = ConnectionString;
				xdoc.Save(configFile);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("Unable to modify configuration file: {0}",
					ex.Message));
			}
		}

		private string BuildConnectionString(string database)
		{
			string connString;

			if (TrustedConnection)
			{
				// IWA
				connString = String.Format(
					"Provider=SQLOLEDB;Data Source={0};Initial Catalog={1};Integrated Security=SSPI",
					_server, database);
			} 
			else
			{
				connString = String.Format(
					"Provider=SQLOLEDB;Data Source={0};Initial Catalog={1};User ID={2};Password={3};Persist Security Info=True",
					_server, database, _userName, _password);
			}

			return connString;
		}

		public string SQLServerServiceInstance()
		{
            string[] regInstances = InstalledInstances();
            if ((regInstances == null) || (regInstances.Length == 0))
                throw new Exception("Cannot find any local SQL Server instances");

			// Add all matching instances that are installed on the machine to an array
			ServiceController[] services = ServiceController.GetServices();
            List<string> instances = new List<string>(regInstances.Length);
			foreach (string instance in regInstances)
			{
				foreach (ServiceController sc in services)
				{
					if ((String.Compare(instance, sc.ServiceName, true) == 0) ||
						(String.Compare("MSSQL$" + instance, sc.ServiceName, true) == 0))   // Shouldn't really be needed
					{
						instances.Add(sc.ServiceName);
						break;
					}
				} 
			}
		
			// Zero - bad news
			if (instances.Count == 0)
				throw new Exception("No suitable SQL Server instances could be found on this machine");

			// One - good news
			if (instances.Count == 1)
				return instances[0];

			// More than one - see if we can match
			string inst = string.Empty;
			if (ServerInstance.Length == 0)
			{
				// Find the default instance
				foreach (string sn in instances)
				{
					if (sn.IndexOf('$') < 0)
					{
						if (inst.Length != 0)
							throw new Exception("More than one SQL Server default instance found");
						inst = sn;
					}
				}
			}
			else
			{
				// Find matching instance
				foreach (string sn in instances)
				{
					if (String.Compare("MSSQL$" + ServerInstance, sn, true) == 0)
					{
						if (inst.Length != 0)
							throw new Exception("More than one SQL Server instance found");
						inst = sn;
					}				
				}
			}

			if (inst.Length == 0)
				throw new Exception("Unable to determine correct SQL Server instance");

			return inst;
		}

		public string ConnectionString
		{
			get
			{
				return BuildConnectionString(_database);
			}
		}

		public bool TrustedConnection
		{
			get
			{
				return (_userName.Length == 0);
			}
		}

		public bool DatabaseExists
		{
			get
			{
				bool created = true;
				try
				{
					TestConnection();
				}
				catch (Exception)
				{
					created = false;
				}
				return created;
			}
		}

		public string Server
		{
			get
			{
				return _server;
			}
		}

		public string Database
		{
			get
			{
				return _database;
			}
		}

		public string UserName
		{
			get
			{
				return _userName;
			}
		}

		public string Password
		{
			get
			{
				return _password;
			}
		}

		public string ServerMachine
		{
			get
			{
				string[] sprt = Server.Split(new char[] {'\\'});
				if (sprt.Length > 0)
					return sprt[0];
				else
                    return Server;
			}
		}

		public string ServerInstance
		{
			get
			{
				string[] sprt = Server.Split(new char[] {'\\'});
				if (sprt.Length == 2)
					return sprt[1];
				else
					return String.Empty;
			}
		}

		public bool LocalServer
		{
			get
			{
                if ((String.Compare(ServerMachine, "(local)", true) == 0) ||
                        (String.Compare(ServerMachine, "localhost", true) == 0) ||
                        (String.Compare(ServerMachine, Environment.MachineName, true) == 0))
                    return true;

                IPHostEntry lentry = Dns.GetHostEntry(Dns.GetHostName());
                IPHostEntry rentry = Dns.GetHostEntry(ServerMachine);

                return (lentry == rentry);
			}
		}

        /// <summary>
        /// Get Installed instances of SQL Server, in Service format
        /// </summary>
        /// <returns>List of instances</returns>
        static public string[] InstalledInstances()
        {
            try
            {
                // 2005 versions onwards
                List<string> instances = new List<string>();
                ManagementObjectSearcher searcher = 
                    new ManagementObjectSearcher("root\\Microsoft\\SqlServer\\ComputerManagement", 
                    "SELECT * FROM SqlServiceAdvancedProperty WHERE PropertyName='SKUNAME'"); 

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    instances.Add((string) queryObj["ServiceName"]);
                }
                return instances.ToArray();
            }
            catch (ManagementException)
            {
                // Fall back to 2000 registry search
                RegistryKey sqlreg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
                if (sqlreg == null)
                    return null;

                string[] instances = sqlreg.GetValue("InstalledInstances") as string[];
                for (int i=0; i<instances.Length; i++)
                {
                    if (String.Compare(instances[i], "MSSQLSERVER", true) != 0) // See http://support.microsoft.com/kb/290991
                        instances[i] = "MSSQL$" + instances[i];
                }
                return instances;
            }
        }

        /// <summary>
        /// Find the 'best' local SQL server
        /// Priority:
        ///  1. Full SQL server instance
        ///  2. Named instance SQLEXPRESS
        ///  3. Any other named instance
        /// </summary>
        /// <returns>SQL Server instance, in service format</returns>
        static public string DefaultInstance()
        {
            string bestInstance = String.Empty;
            string[] instances = InstalledInstances();
            foreach (string instance in instances)
            {
                int pos = instance.IndexOf('$');
                if (pos < 0)
                {
                    bestInstance = instance;        // A full SQL server installation - always use this
                    break;
                }
                else
                {
                    string instName = instance.Substring(pos + 1);
                    if (String.Compare(instName, "SQLEXPRESS", true) == 0)
                        bestInstance = instance;
                    else if (bestInstance.Length == 0)
                        bestInstance = instance;    // The first instance in list
                }
            }
            return bestInstance;
        }

        /// <summary>
        /// Converts a local sql server instance, in service format, to human readable host format
        /// </summary>
        /// <param name="instance">Server instance, e.g. MSSQL$SQLEXPRESS</param>
        /// <returns>Host format, e.g. HOST\SQLEXPRESS</returns>
        static public string ServiceFormatToHostFormat(string instance)
        {
            string machine = System.Environment.MachineName;
            int pos = instance.IndexOf('$');
            if (pos < 0)
                return machine;
            else
                return machine + @"\" + instance.Substring(pos + 1);
        }

		public override string ToString()
		{
			return ConnectionString;
		}

		private readonly string _server;
		private readonly string _database;
		private readonly string _userName;
		private readonly string _password;
	}
}
