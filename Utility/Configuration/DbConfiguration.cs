/*
Database Configuration
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
using System.Diagnostics;

using Aonaware.Utility.Database;

namespace Aonaware.Utility.Configuration
{
	/// <summary>
	/// Store configuration in database.
	/// </summary>
	public class DbConfiguration : Configuration
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="system">System name, as stored in database</param>
		public DbConfiguration(string system) : base(system)
		{
		}

		/// <summary>
		/// Initializes connection to database, reads initial config
		/// </summary>
		/// <param name="connString">Database connection string</param>
		public void Initialize(string connString)
		{
			lock(this)
			{
				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Initializing database configuration, connection:" + connString,
						DbTraceListener.catInfo);

				_conn = new OleDbConnection(connString);

				// Create custom data adaptor
				string sql = "SELECT id, system, attribute, type, val FROM configuration" +
					" WHERE system = ?";
				_dataAdapter = new OleDbDataAdapter(sql, _conn);
				
				// Parameter
				OleDbParameter param = _dataAdapter.SelectCommand.Parameters.Add("@system",
					OleDbType.Char, 16, "system");
				param.Value = System;

				// Insert statement
				sql = "INSERT INTO configuration (system, attribute, type, val)" +
					" VALUES (?,?,?,?); SELECT SCOPE_IDENTITY() AS ID";
				OleDbCommand cmdIns = new OleDbCommand(sql, _conn);
				_dataAdapter.InsertCommand = cmdIns;
				cmdIns.Parameters.Add("system", OleDbType.Char, 16, "system");
				cmdIns.Parameters.Add("attribute", OleDbType.Char, 256, "attribute");
				cmdIns.Parameters.Add("type", OleDbType.Char, 64, "type");
				cmdIns.Parameters.Add("val", OleDbType.VarChar, 1024, "val");
				cmdIns.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;

				// Update statement - optimistic concurrency, id left unmodified
				sql = "UPDATE configuration" + 
					" SET system = ?, attribute = ?, type = ?, val = ?" +
					" WHERE id = ?";
				OleDbCommand cmdUpd = new OleDbCommand(sql, _conn);
				_dataAdapter.UpdateCommand = cmdUpd;
				cmdUpd.Parameters.Add("system_New", OleDbType.Char, 16, "system");
				cmdUpd.Parameters.Add("attribute_New", OleDbType.Char, 256, "attribute");
				cmdUpd.Parameters.Add("type_New", OleDbType.Char, 64, "type");
				cmdUpd.Parameters.Add("val_New", OleDbType.VarChar, 1024, "val");
				OleDbParameter paramUpd = cmdUpd.Parameters.Add("id_Orig", OleDbType.Integer, 0, "id");
				paramUpd.SourceVersion = DataRowVersion.Original;

				// Delete statement - not currently used, again optimistic
				sql = "DELETE FROM configuration" + 
					" WHERE id = ?";
				OleDbCommand cmdDel = new OleDbCommand(sql, _conn);
				_dataAdapter.DeleteCommand = cmdDel;
				OleDbParameter paramDel = cmdDel.Parameters.Add("id_Orig", OleDbType.Integer, 0, "id");
				paramDel.SourceVersion = DataRowVersion.Original;

				LoadFromDatabase();
			}
		}

		/// <summary>
		/// Load configuration information from database
		/// </summary>
		private void LoadFromDatabase()
		{
			lock (this)
			{
				ClearConfig();

				// Open connection if needed
				if (_conn.State != ConnectionState.Open)
					_conn.Open();

				// Read data - Fill dataset
				_dataAdapter.Fill(_configTable);

				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Database configuration read success, rows:" +
						_configTable.Rows.Count.ToString(), DbTraceListener.catInfo);

				// Give connection back
				_conn.Close();
			}
		}

		/// <summary>
		/// Reload configuration information from database
		/// </summary>
		public override void ReloadConfiguration()
		{
			// Can safely assume already initialized
			LoadFromDatabase();
			
			// Let anyone waiting know
			OnConfigurationChanged();
		}

		/// <summary>
		/// Store configuration information in database
		/// </summary>
		public override void StoreConfiguration()
		{
			lock(this)
			{
				if (_configSwitch.TraceVerbose)
					Debug.WriteLine("Storing database configuration information", DbTraceListener.catInfo);

				// Open connection if needed
				if (_conn.State != ConnectionState.Open)
					_conn.Open();

				_dataAdapter.Update(_configTable);
				Modified = false;

				// Close
				_conn.Close();

				// Let clients know
				OnConfigurationChanged();
			}
		}

		static private TraceSwitch _configSwitch = new TraceSwitch("DbConfiguration", "Database configuration trace level");

		private OleDbConnection _conn = null;
		private OleDbDataAdapter _dataAdapter = null;

	}
}
