/*
Syslog Event Arguments
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

namespace Aonaware.Syslog
{
	/// <summary>
	/// Message eventing
	/// </summary>
	public class SyslogEventArgs : EventArgs
	{
		public SyslogEventArgs(IPAddress sourceAddress, SyslogMessage msg)
		{
			_sourceAddress = sourceAddress;
			_msg = msg;
		}

		public SyslogMessage Message
		{
			get
			{
				return _msg;
			}
		}

		public IPAddress SourceAddress
		{
			get
			{
				return _sourceAddress;
			}
		}

		private readonly SyslogMessage _msg;
		private readonly IPAddress _sourceAddress;
	}
}
