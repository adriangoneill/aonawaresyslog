/*
Syslog Shared Data
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

namespace Aonaware.SyslogShared 
{
	/// <summary>
	/// Shared data between client and server
	/// Singleton really..
	/// </summary>
	public sealed class SharedData : MarshalByRefObject
	{
		public SharedData()
		{
		}

		public int Port
		{
			get
			{
				return SyslogConfiguration.Instance.Port;
			}
			set
			{
				SyslogConfiguration.Instance.Port = value;
			}
		}

		public int RetentionPeriod
		{
			get
			{
				return SyslogConfiguration.Instance.RetentionPeriod;
			}
			set
			{
				SyslogConfiguration.Instance.RetentionPeriod = value;
			}
		}

		public void StoreConfiguration()
		{
			SyslogConfiguration.Instance.StoreConfiguration();
		}

		// Singleton lease
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}
