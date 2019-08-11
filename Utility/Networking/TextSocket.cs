/*
Text based Socket communication
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
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Timers;
using System.Diagnostics;

using Aonaware.Utility.Database;

namespace Aonaware.Utility.Networking
{
	/// <summary>
	/// Text based socket interface
	/// </summary>
	public class TextSocket : IDisposable
	{
		public TextSocket()
		{
			_invalidResponseHash = new Hashtable();
		}

		public string HostName
		{
			get
			{
				return _hostName;
			}
			set
			{
				_hostName = value;
				Close();
			}
		}

		public int Port
		{
			get
			{
				return _hostPort;
			}
			set
			{
				_hostPort = value;
				Close();
			}
		}

		public int NoResponseTimeout
		{
			get
			{
				return _noResponseTimeout;
			}
			set
			{
				_noResponseTimeout = value;
			}
		}

		public int IdleTimeout
		{
			get
			{
				lock (this)
				{
					return _idleTimeout;
				}
			}
			set
			{
				lock (this)
				{
					_idleTimeout = value;
				}
			}
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
            if (disposing)
            {
                Close();
            }
        }

        // Use C# destructor syntax for finalization code.
        ~TextSocket()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        public bool Connected
        {
            get
            {
                lock(this)
                {
                    return ((_socket != null) && (_socket.Connected));
                }
            }
        }

		public void Close()
		{
			lock (this)
			{
				if ((_socket != null) && (_socket.Connected))
				{
					if (socketSwitch.TraceVerbose)
						Debug.WriteLine("Attempting to close socket", DbTraceListener.catInfo);

					// Protect from throwing an exception here
					try
					{
						// Let the user do any cleanup
						OnDisconnecting();
						// Close socket
						Abort();
					}
					catch (Exception e)
					{
						if (socketSwitch.TraceWarning)
							Debug.WriteLine("Caught exception closing socket: " + e.Message, DbTraceListener.catWarn);
					}
				}
			}
		}

		public void SetInvalidResponses(int[] invalidResponses)
		{
			foreach (int r in invalidResponses)
				_invalidResponseHash[r] = true;
		}

		private void Connect()
		{
			lock (this)
			{
				// See if already connected
				if (_socket != null)
				{
					if (_socket.Connected)
						return;				// Already connected

					Abort();
				}

				if (socketSwitch.TraceVerbose)
					Debug.WriteLine("Attempting to connect socket, host:" + _hostName, DbTraceListener.catInfo);

				if (_hostName.Length == 0)
					throw new Exception("No dictionary host specified");

				if (_hostPort <= 0)
					throw new Exception("Invalid dictionary port specified");

				IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);

				// Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
				// an exception that occurs when the host IP Address is not compatible with the address family
				// (typical in the IPv6 case).
				foreach(IPAddress address in hostEntry.AddressList)
				{
					IPEndPoint ipe = new IPEndPoint(address, _hostPort);
					Socket tempSocket = 
						new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

					tempSocket.Connect(ipe);

					if(tempSocket.Connected)
					{
						_socket = tempSocket;
						break;
					}
					else
					{
						continue;
					}
				}

				if (_socket == null)
					throw new Exception("Unable to connect to remote host");

				// Set timeout
				_socket.SetSocketOption(SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout, _noResponseTimeout * 1000);
				_socket.SetSocketOption(SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout, _noResponseTimeout * 1000);

				// Create network stream
				_networkStream = new NetworkStream(_socket, true);

				// Create read / write streams
				_streamReader = new StreamReader(_networkStream);
				_streamWriter = new StreamWriter(_networkStream);

				// Allow client to say hello
				OnConnected();

				// Update last time socket used
				SocketUsed();

				// Create idle handler if needed
				if (_idleCheckTimer == null)
				{
					_idleCheckTimer = new System.Timers.Timer();
					_idleCheckTimer.Interval = _idleCheck * 1000;
					_idleCheckTimer.Elapsed +=new ElapsedEventHandler(this.OnIdleEvent);
				}

				// Switch timer on
				_idleCheckTimer.Enabled = true;

				if (socketSwitch.TraceVerbose)
					Debug.WriteLine("Socket connected successfully", DbTraceListener.catInfo);
			}
		}

		private void SocketUsed()
		{
			lock (this)
			{
				_lastSocketFunction = DateTime.UtcNow;
			}
		}

		private void OnIdleEvent(object sender, ElapsedEventArgs e)
		{
			lock (this)
			{
				if ((_socket == null) || !_socket.Connected)
					return;

				TimeSpan diff = DateTime.UtcNow - _lastSocketFunction;
				if (diff.TotalSeconds > _idleTimeout)
					Close();
			}
		}

		/// <summary>
		/// Virtual function called when connected to the socket
		/// </summary>
		public virtual void OnConnected()
		{
			// No implementation
		}

		public virtual void OnDisconnecting()
		{
			// No implementation
		}

		protected void Abort()
		{
			lock (this)
			{
				if (socketSwitch.TraceVerbose)
					Debug.WriteLine("Aborting socket connection", DbTraceListener.catInfo);

				// Close the socket if possible
				if (_socket != null)
				{
					try
					{
						if (_idleCheckTimer != null)
							_idleCheckTimer.Enabled = false;
						_socket.Close();
						if (_streamReader != null)
							_streamReader.Close();
						if (_streamWriter != null)
							_streamWriter.Close();
						if (_networkStream != null)
							_networkStream.Close();
					}
					catch (Exception e)
					{
						if (socketSwitch.TraceWarning)
							Debug.WriteLine("Caught exception aborting socket:" + e.Message, DbTraceListener.catWarn);
					}
					_socket = null;
					_networkStream = null;
					_streamReader = null;
					_streamWriter = null;
				}
			}
		}

		/// <summary>
		/// Abort socket connection throwing message
		/// </summary>
		/// <param name="message">Message to throw</param>
		protected void Abort(string message)
		{
			Abort();
			throw new Exception(message);
		}

		/// <summary>
		/// Receive a line of text from the socket
		/// </summary>
		/// <returns>Line read</returns>
		public string ReceiveLine()
		{	
			string line;

			try
			{
				Connect();
				line = _streamReader.ReadLine();
				if (socketSwitch.TraceVerbose)
					Debug.WriteLine("Socket data <-:" + line, DbTraceListener.catInfo);
				if (line == null)
					Abort("No data received from server");
				SocketUsed();
			}
			catch (Exception e)
			{
				Abort();
				// Throw original exception
				throw e;
			}

			return line;
		}

		/// <summary>
		/// Send a line of text to the socket
		/// </summary>
		/// <param name="line">Line to send</param>
		public void SendLine(string line)
		{
			lock (this)
			{
				try
				{
					Connect();
					if (socketSwitch.TraceVerbose)
						Debug.WriteLine("Socket data ->:" + line, DbTraceListener.catInfo);
					_streamWriter.WriteLine(line);
					_streamWriter.Flush();
					SocketUsed();
				}
				catch (Exception e)
				{
					Abort();
					// Throw original exception
					throw e;
				}
			}
		}

		/// <summary>
		/// Receive a status message from the socket
		/// </summary>
		/// <param name="messageText">Received part after status number</param>
		/// <returns>Status number</returns>
		public int ReceiveStatusMessage(out string messageText)
		{
			string line = ReceiveLine();

			int firstSpace = line.IndexOf(' ');
			if (firstSpace <= 0)
				Abort("Expected status message - received " + line );

			int respId = 0;
			try
			{
				respId = int.Parse(line.Substring(0, firstSpace));
			}
			catch (FormatException)
			{
				Abort("Expected status message - received " + line );
			}

			if (_invalidResponseHash.Contains(respId))
				Abort("Received invalid status " + line);

			if (firstSpace < (line.Length-1))
				messageText = line.Substring(firstSpace + 1);
			else
				messageText = string.Empty;

			return respId;
		}

		/// <summary>
		/// Receive a status message from the socket
		/// </summary>
		/// <returns>Status number</returns>
		public int ReceiveStatusMessage()
		{
			string message;
			return ReceiveStatusMessage(out message);
		}

		/// <summary>
		/// Receive the specified status message
		/// Close socket if unsuccessful
		/// </summary>
		/// <param name="msgExpected">Message expected</param>
		public void ExpectStatusMessage(int msgExpected)
		{
			string messageText;
			int response = ReceiveStatusMessage(out messageText);
			if (response != msgExpected)
				Abort("Required status " + msgExpected.ToString() + " from the server, received "
					+ response.ToString());
		}

		private bool ReceiveMultiLine(out string line)
		{
			line = ReceiveLine();
			if (line == ".")
				return false;
			if ((line.Length >= 2) && (line[0]=='.') && (line[1]=='.'))
				line = line.Substring(1);
			return true;
		}

		/// <summary>
		/// Receive multiple lines of text
		/// </summary>
		/// <returns>Multiline string</returns>
		public string ReceiveText()
		{
			StringBuilder sb = new StringBuilder();
			string line;

			do
			{
				if (!ReceiveMultiLine(out line))
					break;
				sb.Append(line);
				sb.Append(Environment.NewLine);
			} while (true);

			return sb.ToString();
		}

		/// <summary>
		/// Receive multiple lines of text
		/// </summary>
		/// <returns>Array of text</returns>
		public string[] ReceiveTextArray()
		{
			string line;
			ArrayList ar = new ArrayList();

			do
			{
				if (!ReceiveMultiLine(out line))
					break;
				ar.Add(line);
			} while (true);

			string[] ret = new string[ar.Count];
			ar.CopyTo(ret);
			return ret;
		}

		static private TraceSwitch socketSwitch = new TraceSwitch("TextSocket", "Text socket trace level");

		private Hashtable _invalidResponseHash;
		private string _hostName;
		private int _hostPort;
		private int _noResponseTimeout = 15;				// Response timeout in seconds
		private int _idleTimeout = DefaultIdleTimeout;	// Idle time in seconds
		private const int _idleCheck = 5;				// How often to check idleness
		private DateTime _lastSocketFunction;			// UTC time of last socket function
		private System.Timers.Timer _idleCheckTimer = null;

		private Socket _socket = null;
		private NetworkStream _networkStream = null;
		private StreamReader _streamReader = null;
		private StreamWriter _streamWriter= null;

		public const int DefaultIdleTimeout = 60;		// Default idle timeout in seconds
    }
}
