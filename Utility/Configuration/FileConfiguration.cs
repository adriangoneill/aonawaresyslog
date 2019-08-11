/*
File Configuration
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
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Diagnostics;

using Aonaware.Utility.Database;

namespace Aonaware.Utility.Configuration
{
	/// <summary>
	/// Stores user configuration in isolated storage
	/// </summary>
	public class FileConfiguration : Configuration
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="system">System name, as used in filename</param>
		public FileConfiguration(string system) : base(system)
		{
		}

		/// <summary>
		/// Initialize configuration, load initial values from file
		/// </summary>
		public void Initialize()
		{
			lock(this)
			{
				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Loading file configuration", DbTraceListener.catInfo);

				ClearConfig();
				Load(ConfigFileName);
			}
		}

		/// <summary>
		/// Reload configuration information from file
		/// </summary>
		public override void ReloadConfiguration()
		{
			// Can just delegate this to initialize
			Initialize();
			
			// Let anyone waiting know
			OnConfigurationChanged();
		}

		/// <summary>
		/// Store configuration in file
		/// </summary>
		public override void StoreConfiguration()
		{
			lock(this)
			{
				Save(ConfigFileName);
			}
		}

		/// <summary>
		/// Load configuration from specified isolated storage config name
		/// </summary>
		/// <param name="configName">Configuration name</param>
		private void Load(string configName)
		{	
			using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForDomain())
			{
				string[] files = storageFile.GetFileNames(configName);
				bool fileFound = false;
				foreach (string fileName in files)
				{
					if (fileName == configName) 
					{
						fileFound = true;
						break;
					}
				}

				if (fileFound)
				{
					if (_configSwitch.TraceVerbose)
						Debug.WriteLine("Reading configuration store:" + storageFile.ToString(),
							DbTraceListener.catInfo);

					using (StreamReader reader = new StreamReader(
							   new IsolatedStorageFileStream(
							   configName,
							   FileMode.Open,
							   storageFile)))
					{
						_dataSet.ReadXml(reader);
					}
				}
			}

			Modified = false;
		}

		/// <summary>
		/// Save configuration information to isolated storage file
		/// </summary>
		/// <param name="configName">Configuration name</param>
		private void Save(string configName)
		{
			if (!Modified)
				return;
			
			using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForDomain())
			{
				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Loading configuration store:" + storageFile.ToString(),
						DbTraceListener.catInfo);

				using (StreamWriter writer = new StreamWriter(
							new IsolatedStorageFileStream(
							configName,
							FileMode.Create,
							storageFile)))
				{
					_dataSet.WriteXml(writer);
				}
			}

			// Let clients know
			OnConfigurationChanged();

			Modified = false;
		}

		/// <summary>
		/// Configuration file name, based on system name
		/// </summary>
		private string ConfigFileName
		{
			get
			{
				return System + ".xml";
			}
		}

		static private TraceSwitch _configSwitch = new TraceSwitch("FileConfiguration", "File configuration trace level");
	}
}
