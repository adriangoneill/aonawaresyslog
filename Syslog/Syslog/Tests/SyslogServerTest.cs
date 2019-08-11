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
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NUnit.Framework;

namespace Aonaware.Syslog.Tests
{
	/// <summary>
	/// Test the SyslogServer class.
	/// </summary>
	[TestFixture]
	public class SyslogServerTest
	{
		[Test] public void CreateServer()
		{
			SyslogServer srv = new SyslogServer();
			srv.Connect();
			Assert.IsTrue(srv.Connected, "Server connected");
			srv.Close();
		}

		[Test] public void CreateServerUsing()
		{
			using (SyslogServer srv = new SyslogServer())
			{
				srv.Connect();
				Assert.IsTrue(srv.Connected, "Server connected");
			}
		}

		[Test] public void CreateServerTwice()
		{
			using (SyslogServer srv1 = new SyslogServer())
			{
				srv1.Connect();
			}

			using (SyslogServer srv2 = new SyslogServer())
			{
				srv2.Connect();
			}
		}

		[ExpectedException(typeof(SocketException))]
		[Test] public void ServerInUse()
		{
			using (SyslogServer srv1 = new SyslogServer())
			{
				srv1.Connect();
				
				using (SyslogServer srv2 = new SyslogServer())
				{
					srv2.Connect();
				}
			}
		}

		[Test] public void TransferTest()
		{
			using (SyslogServer srv = new SyslogServer())
			{
				srv.SyslogMessageReceived += new SyslogServer.SyslogMessageDelegate
					(OnSyslogMessageReceived);

				srv.Connect();

				_messageCount = 0;
				SyslogMessage msg = new SyslogMessage("localhost", MessageText);

				using (SyslogUdpClient cl = new SyslogUdpClient())
				{
                    cl.Connect(IPAddress.Loopback);

					for (int i=0; i<MessagesToSend; i++)
					{
						cl.Send(msg);
						// Allow a small delay so not to overload
						Thread.Sleep(10);
					}
				}

				// Sleep until message counts are settled
				int prevCount = -1;
				int newCount = 0;
				while (prevCount != newCount)
				{
					Thread.Sleep(100);
					lock (this)
					{
						prevCount = newCount;
						newCount = _messageCount;
					}
				}

				Assert.AreEqual(MessagesToSend, _messageCount, "Messages received");
			}
		}

		private void OnSyslogMessageReceived(Object sender, SyslogEventArgs e)
		{
			lock (this)
			{
				_messageCount++;
			}
		}

		private volatile int _messageCount;

		private const int MessagesToSend = 500;
		private const string MessageText = "Hello world!";
	}
}