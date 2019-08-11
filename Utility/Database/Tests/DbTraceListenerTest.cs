/*
Tests
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
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;

using NUnit.Framework;

namespace Aonaware.Utility.Database.Tests
{
	/// <summary>
	/// Test the DbTraceListener class
	/// </summary>
	[TestFixture]
	public class DbTraceListenerTest
	{
		[TestFixtureSetUp] public void Initialize()
		{
            string db = Properties.Settings.Default.DbConnection;
            if (db == null || db.Length == 0)
			{
				throw new Exception("ConnectionString not found in app config, tests will fail!");
			}

			_conn = new OleDbConnection(db);
			_conn.Open();

			DatabaseLog.Instance.Initialize(db);

			_listener = new DbTraceListener(System);
			Trace.Listeners.Add(_listener);

			// Clear down table
			ClearTable();
		}

		[TestFixtureTearDown] public void Dispose()
		{
			if (_listener != null)
				Trace.Listeners.Remove(_listener);

			// Stop log
			DatabaseLog.Instance.ShutDown();

			// Clear down table
			ClearTable();

			// Close database connection
			if (_conn != null)
				_conn.Close();
		}

		private void ClearTable()
		{
			if (_conn == null)
				throw new Exception("No database connection!");

			const string sql = "DELETE FROM eventLog";
			OleDbCommand cmd = new OleDbCommand(sql, _conn);
			cmd.ExecuteNonQuery();
		}

		[Test] public void TraceTest()
		{
			for (int i=0; i<MessageCount; i++)
			{
				Trace.WriteLine(String.Format("Message {0}", i));
			}

			// Stop db log - means we wait for DB to finish writing
			DatabaseLog.Instance.ShutDown();

			// Ensure counts match
			const string sql = "SELECT COUNT(*) FROM eventLog";
			OleDbCommand cmd = new OleDbCommand(sql, _conn);
			int count = (int) cmd.ExecuteScalar();

			Assert.AreEqual(MessageCount, count, "Messages logged");
		}

		private DbTraceListener _listener = null;
		private OleDbConnection _conn = null;

		private const string System = "TestSystem";
		private const int MessageCount = 100;
	}
}
