/*
Main Service
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
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

using Aonaware.Syslog;
using Aonaware.SyslogShared;
using Aonaware.Utility.Database;
using Aonaware.Utility.Configuration;

namespace Aonaware.SyslogService
{
	public class Service : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Service()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
		}

		// The main entry point for the process
		static void Main(string[] args)
		{
#if DEBUG
			if ((args.Length == 1 && args[0] == "-debug"))
			{
				(new Service()).OnStart(args); // allows easy debugging of OnStart()
				ServiceBase.Run( new Service() );
				return;
			} 
#endif

			System.ServiceProcess.ServiceBase[] ServicesToRun;
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service() };
			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// Service
			// 
			this.ServiceName = SyslogServiceName;

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			// Event log trace
			EventLogTraceListener traceListener = new EventLogTraceListener(SyslogServiceName);
			// Add the event log trace listener to the collection.
			Trace.Listeners.Add(traceListener);

			if (ssSwitch.TraceInfo)
				Trace.WriteLine(SyslogServiceName + " starting", DbTraceListener.catInfo);

			try
			{
				// Initialize database
                string connString = Properties.Settings.Default.DbConnection;
				if (connString == null || connString.Length == 0)
					throw new Exception("No database connection string specified");

				// Connect to database - try a few times should database not be up yet
				int tryCount = 3;
				bool connected = false;
				do
				{
					try
					{
						using (OleDbConnection conn = new OleDbConnection(connString))
						{
							conn.Open();
						}
						connected = true;
					}
					catch (Exception ex)
					{
						if (ssSwitch.TraceError)
							Trace.WriteLine("Could not connect to database: " + ex.Message, 
								DbTraceListener.catError);

						// Sleep for 5 seconds then try again
						Thread.Sleep(5000);
						tryCount--;
					}
				} while (!connected && (tryCount > 0));

				if (!connected)
					throw new Exception("Could not connect to database");

				// Start database log
				DatabaseLog.Instance.Initialize(connString);

				// Load Configuration
				SyslogConfiguration.Instance.Initialize(connString);

				// Clear down messages periodically
				_cleanup = new SyslogMessageCleanup(SyslogConfiguration.Instance.RetentionPeriod);
				_cleanup.Initialize(connString);

				// Create server channel
                int serverPort = Properties.Settings.Default.ServerPort;
				if (serverPort <= 0)
					throw new Exception("Invalid server port specified in configuration file");

                _channel = new HttpChannel(serverPort);
				ChannelServices.RegisterChannel(_channel, false);

				// Create shared object
				_sharedData = new SharedData();

				// Marshall object
				_obj = RemotingServices.Marshal(_sharedData, "SyslogSharedData");

				// Register for updates
				SyslogConfiguration.Instance.ConfigurationChanged +=new 
					Configuration.ConfigurationChangedDel(OnConfigurationChanged);
			}
			catch (Exception ex)
			{
				if (ssSwitch.TraceError)
					Trace.WriteLine("Could not initialize: " + ex.Message, 
						DbTraceListener.catError);
				
				// Fatal - stop right now
				throw;
			}

			try
			{
				Connect();
			}
			catch (Exception ex)
			{
				if (ssSwitch.TraceError)
					Trace.WriteLine(String.Format("Could not start to collect syslog messages on port {0}: {1}",
						SyslogConfiguration.Instance.Port, ex.Message), DbTraceListener.catError);

				// Not fatal - probably port is in use - so carry on regardless
				// Hope for configuration
				Disconnect();
			}
		}
 
		private void Connect()
		{
			// Start logging
			_logger = new SyslogDbLogger();
			int port = SyslogConfiguration.Instance.Port;
			_logger.Port = port;
			_logger.StartLogging();

			_previousPort = SyslogConfiguration.Instance.Port;
			if (ssSwitch.TraceInfo)
				Trace.WriteLine(String.Format("Collecting syslog messages on port {0}",
					port), DbTraceListener.catInfo);
		}

		private void Disconnect()
		{
			if (_logger != null)
			{
				_logger.StopLogging();
				_logger = null;
				_previousPort = -1;
			}
		}

		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			lock (this)
			{
				if (ssSwitch.TraceInfo)
					Trace.WriteLine(SyslogServiceName + " shutting down", DbTraceListener.catInfo);

				try
				{
					Disconnect();
	
					// Close remoting channel
					ChannelServices.UnregisterChannel(_channel);

					// Stop clearing down messages
					_cleanup.ShutDown();

					// Disconnect from database
					DatabaseLog.Instance.ShutDown();
				}
				catch (Exception ex)
				{
					if (ssSwitch.TraceWarning)
						Trace.WriteLine("Error occured while shutting down: " + ex.Message, 
							DbTraceListener.catWarn);
					throw;
				}
			}
		}

		// Configuration changes
		private void OnConfigurationChanged(object sender, EventArgs e)
		{
			lock(this)
			{
				if (ssSwitch.TraceInfo)
					Trace.WriteLine("Configuration changed", DbTraceListener.catInfo);

				int newPort = SyslogConfiguration.Instance.Port;

				// If port has changed, then connect / disconnect
				if ((_logger == null) || (_previousPort != newPort))
				{
					if (ssSwitch.TraceInfo)
						Trace.WriteLine("Configuration - port changed, reconnecting", DbTraceListener.catInfo);

					try
					{
						Disconnect();
						Connect();
					}
					catch (Exception ex)
					{
						if (ssSwitch.TraceError)
							Trace.WriteLine(String.Format("Could not start to collect syslog messages on port {0}: {1}",
								newPort, ex.Message), DbTraceListener.catError);

						// Not fatal - probably port is in use - so again carry on regardless
						Disconnect();
					}
				}
			}
		}

		private SyslogDbLogger _logger = null;
		private int _previousPort = -1;
		private HttpChannel _channel = null;
		private SharedData _sharedData = null;
		private ObjRef _obj = null;
		private SyslogMessageCleanup _cleanup = null;

		public const string SyslogServiceName = "Aonaware Syslog Daemon";

		static private TraceSwitch ssSwitch = new TraceSwitch("SyslogDaemon", "Syslog Daemon trace level");
	}
}
