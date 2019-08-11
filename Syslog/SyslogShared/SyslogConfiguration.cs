/*
Syslog Configuration
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

using Aonaware.Syslog;
using Aonaware.Utility.Configuration;

namespace Aonaware.SyslogShared
{
	/// <summary>
	/// Configuration for the service
	/// </summary>
	public sealed class SyslogConfiguration : DbConfiguration
	{
		// Singleton
		public static SyslogConfiguration Instance
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

			internal static readonly SyslogConfiguration instance = new SyslogConfiguration();
		}

		private SyslogConfiguration() : base("SyslogService")
		{
		}

		public int Port
		{
			get
			{
				return (int) GetValue("Port", SyslogServer.DefaultPort);
			}
			set
			{
				if ((value >= IPEndPoint.MinPort) && (value <= IPEndPoint.MaxPort))
					SetValue("Port", value);
			}
		}

		public int RetentionPeriod
		{
			get
			{
				return (int) GetValue("RetentionPeriod", DefaultRetentionPeriod);
			}
			set
			{
				if ((value >= MinRetentionPeriod) && (value <= MaxRetentionPeriod))
					SetValue("RetentionPeriod", value);
			}
		}

		public const int DefaultRetentionPeriod = 14;
		public const int MinRetentionPeriod = 0;
		public const int MaxRetentionPeriod = 365;
	}
}
