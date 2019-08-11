/*
Syslog Database Logger
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

using Aonaware.Syslog;
using Aonaware.Utility.Database;

namespace Aonaware.SyslogService
{

	/// <summary>
	/// Represents an item to log to the database
	/// </summary>
	public class SyslogDbItem : IDatabaseLog
	{
		public SyslogDbItem(SyslogEventArgs e)
		{
			_e = e;
		}

		#region IDatabaseLog Members

		public OleDbCommand CreateInsertCommand()
		{
			string sql = "INSERT INTO syslog (receivedTime, address, localTime, facility, severity, message)" +
				" VALUES (?,?,?,?,?,?)";
			OleDbCommand cmdIns = new OleDbCommand(sql);
			cmdIns.Parameters.Add("receivedTime", OleDbType.DBTimeStamp, 0, "receivedTime");
			cmdIns.Parameters.Add("address", OleDbType.Char, 25, "address");
			cmdIns.Parameters.Add("localTime", OleDbType.DBTimeStamp, 0, "localTime");
			cmdIns.Parameters.Add("facility", OleDbType.TinyInt, 0, "facility");
			cmdIns.Parameters.Add("severity", OleDbType.TinyInt, 0, "severity");
			cmdIns.Parameters.Add("message", OleDbType.VarChar, 1024, "message");
			return cmdIns;
		}

		public void FillInsertCmd(OleDbCommand cmd)
		{
			OleDbParameterCollection pc = cmd.Parameters;
			pc["receivedTime"].Value = _e.Message.MessageTime;
			pc["address"].Value = _e.SourceAddress.ToString();
			pc["localTime"].Value = _e.Message.LocalTime;
			pc["facility"].Value = (int) _e.Message.Facility;
			pc["severity"].Value = (int) _e.Message.Severity;
			pc["message"].Value = _e.Message.Message;
		}
		#endregion

		private readonly SyslogEventArgs _e;
	}

	/// <summary>
	/// Receive syslog events and log to database
	/// </summary>
	public class SyslogDbLogger
	{
		public SyslogDbLogger()
		{
			_eventDelegate = new Syslog.SyslogServer.SyslogMessageDelegate(OnSyslogMessageReceived);
		}

		public void StartLogging()
		{
			lock (this)
			{
				if (_server != null)
					return;

				try
				{
					// New server
					_server = new SyslogServer(SyslogServer.DefaultAddress, _port);

					// Register for events
					_server.SyslogMessageReceived += _eventDelegate;

					// Start listening
					_server.Connect();
				}
				catch (Exception)
				{
					if (_server != null)
					{
						_server.Close();
						_server.SyslogMessageReceived -= _eventDelegate;
						_server = null;
					}

					throw;
				}
			}
		}

		public void StopLogging()
		{
			lock (this)
			{
				try
				{
					if (_server == null)
						return;

					// Unregister
					_server.SyslogMessageReceived -= _eventDelegate;

					// Shut down
					_server.Close();
					_server = null;
				}
				catch (Exception)
				{
					if (_server != null)
					{
						_server.Close();
						_server = null;
					}

					throw;
				}
			}
		}

		private void OnSyslogMessageReceived(Object sender, SyslogEventArgs e)
		{
			lock (this)
			{
				DatabaseLog.Instance.Log(new SyslogDbItem(e));
			}
		}

		public int Port
		{
			get
			{
				return _port;
			}
			set
			{
				if (_server != null)
					throw new Exception("Cannot change port - already running!");
				_port = value;
			}
		}

		private SyslogServer _server = null;
		private int _port = SyslogServer.DefaultPort;
		SyslogServer.SyslogMessageDelegate _eventDelegate;
	}
}
