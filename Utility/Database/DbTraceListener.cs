/*
Database TraceListener
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
using System.Diagnostics;
using System.Data;
using System.Data.OleDb;

namespace Aonaware.Utility.Database
{
	/// <summary>
	/// TraceListener which writes data to database
	/// </summary>
	/// 

	public class DbTraceListener : TraceListener
	{
		private class DbTraceListenerEvent : IDatabaseLog
		{
			public DbTraceListenerEvent(string system, string category, string message)
			{
				_system = system;
				_category = category;
				_message = message;
				_eventTime = DateTime.UtcNow;
			}

			#region IDatabaseLog Members

			public OleDbCommand CreateInsertCommand()
			{
				const string insertCmd =
				"INSERT INTO eventLog " +
				"(eventTime, system, category, message) " +
				"VALUES (?, ?, ?, ?)";

				OleDbCommand cmd = new OleDbCommand(insertCmd);
				OleDbParameterCollection pc = cmd.Parameters;

				pc.Add("eventTime", OleDbType.DBTimeStamp, 0, "eventTime");
				pc.Add("system", OleDbType.Char, 16, "system");
				pc.Add("category", OleDbType.Char, 16, "category");
				pc.Add("message", OleDbType.VarChar, 512, "message");
				return cmd;
			}

			public void FillInsertCmd(OleDbCommand cmd)
			{
				OleDbParameterCollection pc = cmd.Parameters;
				pc["eventTime"].Value = _eventTime;
				pc["system"].Value = _system;
				pc["category"].Value = _category;
				pc["message"].Value = _message;
			}

			#endregion

			private readonly DateTime _eventTime;
			private readonly string _system;
			private readonly string _category;
			private readonly string _message;
		}

		public DbTraceListener(string system)
		{
			_system = system;
		}

		public override void Write(string message)
		{
			WriteLine(message, string.Empty);
		}

		public override void Write(string message, string category)
		{
			WriteLine(message, category);
		}

		public override void WriteLine(string message)
		{
			WriteLine(message, string.Empty);
		}

		public override void WriteLine(string message, string category)
		{
			try
			{
				DbTraceListenerEvent ev = new DbTraceListenerEvent(_system, category, message);
				DatabaseLog.Instance.Log(ev, false);
			}
			catch (Exception)
			{
				// Do nothing (unfortunately!)
			}
		}

		private readonly string _system;

		public const string catInfo		= "Info";
		public const string catWarn		= "Warning";
		public const string catError	= "Error";
	}
}
