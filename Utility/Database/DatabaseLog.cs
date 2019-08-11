/*
Asynchronous database logging
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
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace Aonaware.Utility.Database
{
	/// <summary>
	/// Log information to database asynchronously and efficiently.
	/// </summary>
	public sealed class DatabaseLog
	{
		// Singleton
		public static DatabaseLog Instance
		{
			get
			{
				return Nested.instance;
			}
		}

		class Nested
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly DatabaseLog instance = new DatabaseLog();
		}

		private DatabaseLog()
		{
			_eventQueueItem = new AutoResetEvent(false);
			_eventThreadReady = new AutoResetEvent(false);
			_eventQueue = new List<DatabaseLogItem>();
            _insertCmdHash = new Dictionary<Type, OleDbCommand>();
		}

		public void Initialize(string connString)
		{
			lock(this)
			{
				if (_logThread != null)
					return;				// Already running

				_connectionString = connString;

				if (dbLogSwitch.TraceVerbose)
					Debug.WriteLine("Database log initializing", DbTraceListener.catInfo);

				// Clear existing variables
				_exiting = false;
				_eventQueue.Clear();
				_insertCmdHash.Clear();

				_logThread = new Thread(new ThreadStart(ThreadFunction));
				_logThread.Start();

				// Wait for thread to start and initialize.  Means events won't get thrown away.
				_eventThreadReady.WaitOne();
			}
		}

		public void ShutDown()
		{
			lock (this)
			{
				if (_logThread == null)
					return;

				if (dbLogSwitch.TraceVerbose)
					Debug.WriteLine("Database log shutting down...", DbTraceListener.catInfo);

				lock (_eventQueue)
				{
					_exiting = true;
					_eventQueueItem.Set();
				}

				// Wait for completion
				_logThread.Join();
				_logThread = null;
			}

			if (dbLogSwitch.TraceVerbose)
				Debug.WriteLine("Database log shutdown complete", DbTraceListener.catInfo);
		}

		public void Log(IDatabaseLog ev)
		{
			Log(ev, true);
		}

		public void Log(IDatabaseLog ev, bool traceErrors)
		{
			lock (this)
			{
				if (_logThread == null)
				{
					if (dbLogSwitch.TraceWarning && traceErrors)
						Debug.WriteLine("Unable to log to database as system not running", DbTraceListener.catInfo);

					throw new Exception("Unable to log to database as system not running");
				}

				lock (_eventQueue)
				{
					_eventQueue.Add(new DatabaseLogItem(ev, traceErrors));
					_eventQueueItem.Set();
				}
			}
		}

		private void ThreadFunction()
		{
			if (dbLogSwitch.TraceVerbose)
				Debug.WriteLine("Database logging thread initialized", DbTraceListener.catInfo);

			using (OleDbConnection conn = new OleDbConnection(_connectionString))
			{
				_eventThreadReady.Set();

				bool exiting = false;

				do
				{
					_eventQueueItem.WaitOne();

					// Copy items / state to local queue
					DatabaseLogItem[] localQueue;
					lock (_eventQueue)
					{
						localQueue = new DatabaseLogItem[_eventQueue.Count];
						_eventQueue.CopyTo(localQueue);
						_eventQueue.Clear();
					
						exiting = _exiting;
					}

					bool traceErrors = true;

					try
					{
						if (conn.State != ConnectionState.Open)
							conn.Open();

						foreach (DatabaseLogItem i in localQueue)
						{
							try
							{
								IDatabaseLog r = i._item;

								if (!i._traceErrors)
									traceErrors = false;

								// Dynamically create insert command if we've never processed
								// this type of object before
								OleDbCommand insCmd;
                                _insertCmdHash.TryGetValue(r.GetType(), out insCmd);
								if (insCmd == null)
								{
									insCmd = r.CreateInsertCommand();
									insCmd.Connection = conn;
									_insertCmdHash[r.GetType()] = insCmd;
								}

								r.FillInsertCmd(insCmd);
								insCmd.ExecuteNonQuery();
							}
							catch (Exception ex)
							{
								if (dbLogSwitch.TraceError)
									if (i._traceErrors)
										Debug.WriteLine("Unable to log to database:" + ex.Message,
											DbTraceListener.catError);
								continue;
							}
						}

						if (dbLogSwitch.TraceVerbose && traceErrors)
							Debug.WriteLine(String.Format("{0} status message(s) logged sucecssfully to database",
								localQueue.Length), DbTraceListener.catInfo);

						conn.Close();
					}
					catch (Exception ex)
					{
						if (dbLogSwitch.TraceError && traceErrors)
							Debug.WriteLine("Unable to connect to database:" + ex.Message,
								DbTraceListener.catError);
					}

				} while (!exiting);
			}
		}

		private class DatabaseLogItem
		{
			public DatabaseLogItem(IDatabaseLog item, bool traceErrors)
			{
				_item = item;
				_traceErrors = traceErrors;
			}

			public readonly IDatabaseLog _item;
			public readonly bool _traceErrors;
		}

		static private TraceSwitch dbLogSwitch = new TraceSwitch("DbLog", "Database logging trace level");

		private Thread _logThread = null;
		private string _connectionString;
		private AutoResetEvent _eventQueueItem;
		private AutoResetEvent _eventThreadReady;
		private bool _exiting;
		private List<DatabaseLogItem> _eventQueue;
		private Dictionary<Type, OleDbCommand> _insertCmdHash;
	}

	public interface IDatabaseLog
	{
		OleDbCommand CreateInsertCommand();
		void FillInsertCmd(OleDbCommand cmd);
	}
}
