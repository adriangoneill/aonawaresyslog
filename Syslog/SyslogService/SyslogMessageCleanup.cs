/*
Syslog Message Cleanup
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
using System.Timers;

using Aonaware.SyslogShared;
using Aonaware.Utility.Database;

namespace Aonaware.SyslogService
{
	/// <summary>
	/// Cleans up syslog messages periodically
	/// </summary>
	public class SyslogMessageCleanup
	{
		public SyslogMessageCleanup(int retentionPeriod)
		{
			_retentionPeriod = retentionPeriod;
		}

		public void Initialize(string connString)
		{
			lock(this)
			{
				if (scSwitch.TraceVerbose)
					Trace.WriteLine("Initializing message cleanup system, connection:" + connString,
						DbTraceListener.catInfo);

				_conn = new OleDbConnection(connString);

				// Create custom data deletor
				string sql = "DELETE FROM syslog WHERE receivedTime < ?";
				_delCmd = new OleDbCommand(sql, _conn);
				_delCmd.Parameters.Add("receivedTime", OleDbType.DBTimeStamp, 0, "receivedTime");

				// Create timer to check every so often
				_timer = new Timer(_timerPeriod);
				_timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
				_timer.Enabled = true;
			}
		}

		private void OnTimerEvent(object source, ElapsedEventArgs e)
		{
			lock (this)
			{
				// Zero means keep everything (!)
				if (_retentionPeriod <= 0)
					return;

				try
				{
					DateTime past = DateTime.UtcNow.AddDays(-_retentionPeriod);

					// Open connection if needed
					if (_conn.State != ConnectionState.Open)
						_conn.Open();

					// Delete rows!
					_delCmd.Parameters["receivedTime"].Value = past;
					int rows = _delCmd.ExecuteNonQuery();

					// Give connection back
					_conn.Close();

					if (scSwitch.TraceVerbose && (rows > 0))
						Trace.WriteLine(String.Format("Syslog message cleanup, {0} row(s) deleted", rows),
							DbTraceListener.catInfo);
				}
				catch (Exception ex)
				{
					if (scSwitch.TraceWarning)
						Trace.WriteLine("Could not clean up syslog messages: " + ex.Message,
							DbTraceListener.catWarn);
				}
			}
		}

		public void ShutDown()
		{
			lock (this)
			{
				_timer.Enabled = false;
				_timer = null;
				_conn = null;
			}
		}

		private int RetentionPeriod
		{
			get
			{
				lock (this)
				{
					return _retentionPeriod;
				}
			}
			set
			{
				lock (this)
				{
					if ((value < SyslogConfiguration.MinRetentionPeriod) 
							|| (value > SyslogConfiguration.MaxRetentionPeriod))
						throw new Exception("Invalid retention period specified");
					_retentionPeriod = value;
				}
			}
		}

		private int _retentionPeriod;
		private OleDbConnection _conn;
		private OleDbCommand _delCmd;
		private Timer _timer;

		private const int _timerPeriod = 1000 * 60 * 10;		// 10 mins

		static private TraceSwitch scSwitch = new TraceSwitch("MessageCleanup",
				"Message Retention Cleanup trace level");
	}
}
