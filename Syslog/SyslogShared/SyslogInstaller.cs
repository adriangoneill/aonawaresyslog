/*
Syslog Installer
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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Install;

using Aonaware.Utility.Database;

namespace Aonaware.SyslogShared
{
	/// <summary>
	/// Base class for both syslog installation classes
	/// </summary>
	public class SyslogInstaller : System.Configuration.Install.Installer
	{
		public SyslogInstaller()
		{
		}

		protected DatabaseInstaller GetSystemInstaller()
		{
			return GetInstaller(true);
		}

		protected DatabaseInstaller GetUserInstaller()
		{
			return GetInstaller(false);
		}

		private DatabaseInstaller GetInstaller(bool isSystem)
		{
			if (Context == null)
				throw new Exception("No installation context available");

			StringDictionary par = Context.Parameters;

			if (!par.ContainsKey(_serverString))
				throw new InstallException("No Server specified during installation");
			string server = par[_serverString];

			if (!par.ContainsKey(_dataString))
				throw new InstallException("No Database specified during installation");
			string data = par[_dataString];

			string userString, passString;
			if (isSystem)
			{
				userString = _userString;
				passString = _passString;
			}
			else
			{
				userString = _ruserString;
				passString = _rpassString;
			}

			string user = string.Empty;
			string pass = string.Empty;

			if (par.ContainsKey(userString))
			{
				user = par[userString];

				if (par.ContainsKey(passString))
					pass = par[passString];
			}

			return new DatabaseInstaller(server, data, user, pass);
		}

		protected string TargetDirectory
		{
			get
			{
				if (Context == null)
					throw new Exception("No installation context available");

				if (!Context.Parameters.ContainsKey(_targetString))
					throw new InstallException("No Target specified during installation");
				return Context.Parameters[_targetString];
			}
		}

		protected bool SkipDatabaseInstallation
		{
			get
			{
				// Determine if database installation can be skipped
				if ((Context != null) && (Context.Parameters.ContainsKey(_skipString)))
				{
					if (Context.Parameters[_skipString] == "1")
					{
						return true;
					}
				}
				return false;
			}
		}

		protected const string _serverString	= "server";
		protected const string _dataString		= "data";
		protected const string _userString		= "user";
		protected const string _passString		= "pass";
		protected const string _ruserString		= "ruser";
		protected const string _rpassString		= "rpass";
		protected const string _skipString		= "skip";
		protected const string _targetString	= "target";
	}
}
