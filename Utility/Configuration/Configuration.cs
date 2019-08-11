/*
Configuration
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
using System.Diagnostics;

using Aonaware.Utility.Database;

namespace Aonaware.Utility.Configuration
{
	/// <summary>
	/// Abstract class representing configuration data.
	/// File and database configuration inherits from this
	/// </summary>
	public abstract class Configuration
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="system">System name, as used in database / file</param>
		protected Configuration(string system)
		{
			if (_configSwitch.TraceVerbose)
				Debug.WriteLine("Initializing configuration, _system:" + _system, DbTraceListener.catInfo);

			_system = system;

			// Set up database table mappings
			_dataSet = new DataSet();
			_configTable = _dataSet.Tables.Add("configuration");

			// id (autoincrement) column
			DataColumn id = _configTable.Columns.Add("id", typeof(int));
			id.AutoIncrement = true;
			id.AutoIncrementSeed = -1;
			id.AutoIncrementStep = -1;
			id.AllowDBNull = false;
			id.Unique = true;
			
			DataColumn sys = _configTable.Columns.Add("system", typeof(string));
			sys.AllowDBNull = false;
			sys.MaxLength = 16;

			DataColumn attribute = _configTable.Columns.Add("attribute", typeof(string));
			attribute.AllowDBNull = false;
			attribute.MaxLength = 256;

			DataColumn type = _configTable.Columns.Add("type", typeof(string));
			type.AllowDBNull = false;
			type.MaxLength = 64;

			DataColumn val = _configTable.Columns.Add("val", typeof(string));
			val.AllowDBNull = false;
			val.MaxLength = 1024;

			// Primary Key
			_configTable.PrimaryKey = new DataColumn[] { id };

			// Constraint
			DataColumn[] cols = new DataColumn[] { sys, attribute };
			_configTable.Constraints.Add(new UniqueConstraint(cols));

			_modified = false;
		}

		/// <summary>
		/// Refresh configuration.  Base classes must call back up.
		/// </summary>
		public abstract void ReloadConfiguration();

		/// <summary>
		/// Abstract method for storing.  Callee responsible for custom initialization.
		/// </summary>
		public abstract void StoreConfiguration();

		// These functions are all quite inefficient - need rewriting to 
		// use hash objects

		/// <summary>
		/// Get a specific value from the configuration store
		/// </summary>
		/// <param name="setting">Config setting to get</param>
		/// <returns>Config value, or null if not found</returns>
		public object GetValue(string setting)
		{
			lock(this)
			{
				DataRow[] rows = _configTable.Select("attribute = '" + setting + "'"); 
				
				if (rows.Length == 0)
					return null;

				if (rows.Length > 1)
					throw new Exception("Read " + rows.Length + " rows for value " + setting +
						", should only be one row");

				string typeAsString = ((string) rows[0]["type"]).Trim();
				string val = (string) rows[0]["val"];

				Type type = Type.GetType(typeAsString);
				if (type == null)
					throw new Exception("Cannot instantiate type " + typeAsString);
				
				Object o = Convert.ChangeType(val, type);

				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Read configuration item " + setting + " as " + val, DbTraceListener.catInfo);

				return o;
			}
		}

		/// <summary>
		/// Store a value in the configuration store
		/// </summary>
		/// <param name="setting">Config value to set</param>
		/// <param name="val">Object to store (as text)</param>
		public void SetValue(string setting, object val)
		{
			lock(this)
			{
				string valAsString = Convert.ToString(val);

				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Setting configuration item " + setting + " to " + valAsString,
						DbTraceListener.catInfo);

				DataRow[] rows = _configTable.Select("attribute = '" + setting + "'");
				if (rows.Length == 0) 
				{
					DataRow row = _configTable.NewRow();
					row["system"] = System;
					row["attribute"] = setting;
					row["type"] = val.GetType().ToString();
					row["val"] = valAsString;
					_configTable.Rows.Add(row);
				}
				else
				{
					DataRow row = rows[0];
					// Return if data unchanged
					if (valAsString == (string) row["val"])
						return;

					row["type"] = val.GetType().ToString();
					row["val"] = valAsString;
				}

				Modified = true;
			}
		}

		/// <summary>
		/// Returns a default value if types differ or no data
		/// </summary>
		/// <param name="setting">Config value to get</param>
		/// <param name="defaultValue">Default value to return if not found</param>
		/// <returns>Either the value from the store or the default value</returns>
		public object GetValue(string setting, object defaultValue)
		{
			object o = null;
			try
			{
				o = GetValue(setting);
			}
			catch (Exception e)
			{
				if (_configSwitch.TraceError)
					Debug.WriteLine("Caught exception getting config value, " + e.Message, DbTraceListener.catError);
			}

			if (o == null || o.GetType() != defaultValue.GetType())
				return defaultValue;

			return o;
		}

		/// <summary>
		/// Indicates if configuration store has been modified
		/// </summary>
		public bool Modified
		{
			get
			{
				lock(this)
				{
					return _modified;
				}
			}
			set
			{
				lock(this)
				{
					_modified = value;
				}
			}
		}

		/// <summary>
		/// System name, as specified in constructor
		/// </summary>
		protected string System
		{
			get
			{
				return _system;
			}
		}

		/// <summary>
		/// Delegate which reports configuration changes
		/// </summary>
		public delegate void ConfigurationChangedDel(object sender, EventArgs e);

		/// <summary>
		/// Event which receives configuration changed messages
		/// </summary>
		public event ConfigurationChangedDel ConfigurationChanged;

		/// <summary>
		/// Internal method called when configuration changed and saved
		/// </summary>
		protected void OnConfigurationChanged()
		{
			if (ConfigurationChanged != null)
				ConfigurationChanged(this, null);
		}

		protected void ClearConfig()
		{
			_dataSet.Clear();
		}

		private readonly string _system;
		private bool _modified;
		static private TraceSwitch _configSwitch = new TraceSwitch("Configuration", "User configuration trace level");

		protected DataSet _dataSet;
		protected DataTable _configTable;
	}
}
