/*
Syslog Server
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;

using Aonaware.Utility.Database;

namespace Aonaware.Syslog
{
	/// <summary>
	/// Receives, decodes and dispatches syslog messages to anyone who cares
	/// </summary>
	public class SyslogServer : IDisposable
	{
		public SyslogServer()
		{
			_listenPoint = new IPEndPoint(DefaultAddress, DefaultPort);
		}

		public SyslogServer(IPAddress address, int port)
		{
			_listenPoint = new IPEndPoint(address, port);
		}

		public void Connect()
		{
			lock(this)
			{
				if (_disposed)
					throw new ObjectDisposedException("SyslogServer");

				if (_listenThread != null)
					return;

				try
				{
					_recUDPClient = new UdpClient(_listenPoint);

					_listenThread = new Thread(new ThreadStart(ThreadProc));
					_listenThread.Start();

					if (ssSwitch.TraceVerbose)
						Trace.WriteLine(String.Format("Started collecting messages at endpoint {0}",
							_listenPoint), DbTraceListener.catInfo);
				}
				catch (Exception ex)
				{
					if (ssSwitch.TraceError)
						Trace.WriteLine(String.Format("Could not listen for messages: {0}",
							ex.Message), DbTraceListener.catError);

					Close();
					throw;
				}
			}
		}

		public void Close()
		{
			lock (this)
			{
				if (_listenThread == null)
					return;

				try
				{
					_listenThread.Abort();
					_recUDPClient.Close();
					_listenThread.Join();

					if (ssSwitch.TraceVerbose)
						Trace.WriteLine("Stopped collecting messages",
							DbTraceListener.catInfo);
				}
				finally
				{
					_listenThread = null;
					_recUDPClient = null;
				}
			}
		}

		public bool Connected
		{
			get
			{
				lock (this)
				{
					return (_listenThread != null);
				}
			}
		}

		private void OnSyslogMessageReceived(IPAddress sourceAddress, SyslogMessage msg)
		{
			if (SyslogMessageReceived != null)
				SyslogMessageReceived(this, new SyslogEventArgs(sourceAddress, msg));
		}

		private void ThreadProc()
		{
			bool exiting = false;
			do
			{
				try
				{
					IPEndPoint remoteHost = _listenPoint;
					while (true)
					{
						Byte[] receiveBytes = _recUDPClient.Receive(ref remoteHost);
						string returnData = Encoding.ASCII.GetString(receiveBytes);
						SyslogMessage msg = SyslogMessage.Parse(remoteHost.Address, 
							returnData);

						// Fire event
						OnSyslogMessageReceived(remoteHost.Address, msg);
					}
				}
				catch (ThreadAbortException)
				{
					exiting = true;
					if (ssSwitch.TraceVerbose)
						Trace.WriteLine("Message collection thread shutting down",
							DbTraceListener.catInfo);
				}
				catch (Exception ex)
				{
					if (ssSwitch.TraceError)
						Trace.WriteLine(String.Format("Error reciving syslog message: {0}",
							ex.Message), DbTraceListener.catError);
				}
			} while (!exiting);
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!_disposed)
			{
				if(disposing)
				{
					Close();
				}
				_disposed = true;
			}
		}

		~SyslogServer()      
		{
			Dispose(false);
		}

		// Delegate information
		public delegate void SyslogMessageDelegate(Object sender, SyslogEventArgs e);
		public event SyslogMessageDelegate SyslogMessageReceived;

		private bool _disposed = false;

		public const int DefaultPort = 514;
		public static readonly IPAddress DefaultAddress = IPAddress.Any;

		private readonly IPEndPoint _listenPoint;
		private UdpClient _recUDPClient;

		private Thread _listenThread = null;

		static private TraceSwitch ssSwitch = new TraceSwitch("SyslogServer", "Syslog Server trace level");
	}
}
