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
using System.Threading;

using NUnit.Framework;

namespace Aonaware.Utility.Database.Tests
{
	/// <summary>
	/// Test the DatabaseLog class.
	/// </summary>
	[TestFixture]
	public class DatabaseLogTest
	{
		private class DatabaseLogTestItem : IDatabaseLog
		{
			public DatabaseLogTestItem(DateTime logTime, string ev)
			{
				_logTime = logTime;
				_ev = ev;
			}

			public virtual OleDbCommand CreateInsertCommand()
			{
				const string insertCmd =
						  "INSERT INTO sampleLog " +
						  "(logTime, event) " +
						  "VALUES (?, ?)";

				OleDbCommand cmd = new OleDbCommand(insertCmd);
				OleDbParameterCollection pc = cmd.Parameters;

				pc.Add("logTime", OleDbType.DBTimeStamp, 0, "logTime");
				pc.Add("event", OleDbType.Char, 32, "event");
				return cmd;
			}

			public virtual void FillInsertCmd(OleDbCommand cmd)
			{
				OleDbParameterCollection pc = cmd.Parameters;
				pc["logTime"].Value = _logTime;
				pc["event"].Value = _ev;
			}

			private readonly DateTime _logTime;
			private readonly string _ev;
		}

		private string DbConn()
		{
            string db = Properties.Settings.Default.DbConnection;
			if (db == null || db.Length == 0)
			{
				throw new Exception("ConnectionString not found in app config, tests will fail!");
			}

			return db;
		}

		[TestFixtureSetUp] public void Initialize()
		{
			_conn = new OleDbConnection(DbConn());
			_conn.Open();

			// Clear down table
			ClearTable();

			// Event
			_startEvent = new ManualResetEvent(false);
		}

		[TestFixtureTearDown] public void Dispose()
		{
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

			const string sql = "DELETE FROM sampleLog";
			OleDbCommand cmd = new OleDbCommand(sql, _conn);
			cmd.ExecuteNonQuery();
		}

		private int RowCount()
		{
			if (_conn == null)
				throw new Exception("No database connection!");

			const string sql = "SELECT COUNT(*) FROM sampleLog";
			OleDbCommand cmd = new OleDbCommand(sql, _conn);
			return (int) cmd.ExecuteScalar();
		}

		[Test] public void LogData()
		{
			// Initialize
			DatabaseLog.Instance.Initialize(DbConn());
			int oldRowCount = RowCount();

			Thread[] threads = new Thread[ThreadCount];

			_runningCount = 0;

			for (int i=0; i<ThreadCount; i++)
			{
				threads[i] = new Thread(new ThreadStart(ThreadFunction));
				threads[i].Name = i.ToString();
				threads[i].Start();
			}

			// Wait for all threads to start
			lock(this)
			{
				while (_runningCount < ThreadCount)
					Monitor.Wait(this, Timeout.Infinite);
			}

			// Kick off all at once
			_startEvent.Set();

			// Wait for completion
			for (int i=0; i<ThreadCount; i++)
			{
				threads[i].Join();
			}

			Console.WriteLine("All threads done, shutting down!");

			// Shutdown - means we wait
			DatabaseLog.Instance.ShutDown();

			// Ensure counts match
			Assert.AreEqual(ThreadCount * EventCount, RowCount() - oldRowCount,
				"Messages logged");
		}

		private void ThreadFunction()
		{
			// Signal started
			lock (this)
			{
				_runningCount++;
				Monitor.Pulse(this);
			}

			// Wait for signal
			_startEvent.WaitOne();

			Console.WriteLine("Thread {0} starting", Thread.CurrentThread.Name);

			for (int i=0; i<EventCount; i++)
			{
				string ev = String.Format("Thread #{0}, Event #{1}",
					Thread.CurrentThread.Name, i);
				DatabaseLogTestItem ti = new DatabaseLogTestItem(DateTime.Now, ev);
				DatabaseLog.Instance.Log(ti);
				Thread.Sleep(0);
			}

			Console.WriteLine("Thread {0} exiting", Thread.CurrentThread.Name);
		}

		[Test] public void InitTwice()
		{
			// Initialize twice
			int oldRowCount = RowCount();

			DatabaseLog.Instance.Initialize(DbConn());
			DatabaseLog.Instance.Log(new DatabaseLogTestItem(DateTime.Now, "Testing"));
			DatabaseLog.Instance.ShutDown();

			DatabaseLog.Instance.Initialize(DbConn());
			DatabaseLog.Instance.Log(new DatabaseLogTestItem(DateTime.Now, "Testing"));
			DatabaseLog.Instance.ShutDown();

			Assert.AreEqual(2, RowCount() - oldRowCount, "Rowcount after initializing twice");
		}

		private ManualResetEvent _startEvent;

		private OleDbConnection _conn = null;
		private int _runningCount;

		private const int ThreadCount = 16;
		private const int EventCount = 100;
	}
}
