/*
Syslog Message
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
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Aonaware.Syslog
{
	/// <summary>
	/// A Syslog message, and parsing.
	/// </summary>
	public class SyslogMessage
	{
		public enum FacilityCode
		{
			Kernel, UserLevel, MailSystem, SystemDaemon, Security1,
			SyslogdInternal, LinePrinter, NetworkNews, UUCP, Clock1,
			Security2, FTP, NTP, LogAudit, LogAlert,
			Clock2, Local0, Local1, Local2, Local3,
			Local4, Local5, Local6, Local7
		}

		public enum SeverityCode
		{
			Emergency, Alert, Critical, Error, Warning,
			Notice, Informational, Debug
		}

		public SyslogMessage(string hostName, string msg)
		{
			ValidateHost(hostName);
			ValidateMessage(msg);

			_messageTime = DateTime.UtcNow;
			_facility = DefaultFacility;
			_severity = DefaultSeverity;
			_localTime = DateTime.Now;
			_localHost = hostName;
			_message = msg;
		}

		public SyslogMessage(string hostName, string msg, FacilityCode fc,
			SeverityCode sc, DateTime localTime)
		{
			ValidateHost(hostName);
			ValidateMessage(msg);

			_messageTime = DateTime.UtcNow;
			_facility = fc;
			_severity = sc;
			_localTime = localTime;
			_localHost = hostName;
			_message = msg;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('<');
			sb.Append(Priority);
			sb.Append('>');
			if (LocalTime.Day < 10)
			{
				sb.Append(LocalTime.ToString(_dateFormat2, _standardCulture));
			}
			else
			{
				sb.Append(LocalTime.ToString(_dateFormat1, _standardCulture));
			}
			sb.Append(' ');
			sb.Append(LocalHost);
			sb.Append(' ');
			sb.Append(Message);
			if (sb.Length > MaxPacketLength)
				sb.Length = MaxPacketLength;
			return sb.ToString();
		}

		private void ValidateHost(string host)
		{
			if (!_hostFormat.IsMatch(host))
				throw new ArgumentException("Invalid format for hostname", "hostName");
		}

		private void ValidateMessage(string msg)
		{
			if (_msgInvalidChars.IsMatch(msg))
				throw new ArgumentException("Invalid format for message", "msg");
		}

		private static void ParseCode(string code, out FacilityCode fc, out SeverityCode sc)
		{
			int numericCode = Convert.ToInt32(code);
			int facilityCode = numericCode / 8;
			int severityCode = numericCode % 8;

			if (!Enum.IsDefined(typeof(FacilityCode), facilityCode))
				throw new ArgumentException(String.Format("Invalid facility code {0}", facilityCode),
					"code");
			fc = (FacilityCode) facilityCode;

			if (!Enum.IsDefined(typeof(SeverityCode), severityCode))
				throw new ArgumentException(String.Format("Invalid severity code {0}", severityCode),
					"code");
			sc = (SeverityCode) severityCode;
		}

		public static SyslogMessage Parse(IPAddress hostIP, string syslogString)
		{
			FacilityCode fc;
			SeverityCode sc;
			DateTime recdTime;
			string msg;

			// Strip out all non-printable characters - replace with spaces.
			string strippedString = _msgInvalidChars.Replace(syslogString, " ");

			try
			{
				// Try parse PRI, date and message
				// Note - hostname not matched as very few devices conform to RFC
				Match m = _fullFormat.Match(strippedString);
				if ((m != null) && m.Success && (m.Groups.Count == 4))
				{
					ParseCode(m.Groups[1].ToString(), out fc, out sc);

					// Try to parse both date formats
					try
					{
						recdTime = DateTime.ParseExact(m.Groups[2].ToString(), _dateFormat1, _standardCulture);
					}
					catch (FormatException)
					{
						recdTime = DateTime.ParseExact(m.Groups[2].ToString(), _dateFormat2, _standardCulture);
					}

					msg = m.Groups[3].ToString();
					return new SyslogMessage(hostIP.ToString(), msg, fc, sc, recdTime);
				} 
				else 
				{
					throw new Exception("Cannot parse message");
				}
			}
			catch (Exception)
			{
				// Try and parse PRI only
				try
				{
					Match m = _priFormat.Match(strippedString);
					if ((m != null) && m.Success && (m.Groups.Count == 3))
					{
						ParseCode(m.Groups[1].ToString(), out fc, out sc);
						msg = m.Groups[2].ToString();
						SyslogMessage sm = new SyslogMessage(hostIP.ToString(), msg);
						sm._facility = fc;
						sm._severity = sc;
						return sm;
					}
					else 
					{
						throw new Exception("Cannot parse message");
					}
				}

				catch (Exception)
				{
					// Cannot decode at all
					return new SyslogMessage(hostIP.ToString(), strippedString);
				}
			}
		}

		public DateTime MessageTime
		{
			get
			{
				return _messageTime;
			}
		}

		public FacilityCode Facility
		{
			get
			{
				return _facility;
			}
		}

		public SeverityCode Severity
		{
			get
			{
				return _severity;
			}
		}

		public DateTime LocalTime
		{
			get
			{
				return _localTime;
			}
		}

		public string LocalHost
		{
			get
			{
				return _localHost;
			}
		}

		public string Message
		{
			get
			{
				return _message;
			}
		}

		public int Priority
		{
			get
			{
				return ((int) Facility * 8) + ((int) Severity);
			}
		}

		public const int MaxPacketLength = 1024;
		public const FacilityCode DefaultFacility = FacilityCode.UserLevel;
		public const SeverityCode DefaultSeverity = SeverityCode.Notice;

		private DateTime _messageTime;
		private FacilityCode _facility;
		private SeverityCode _severity;
		private DateTime _localTime;
		private string _localHost;
		private string _message;
		private const string _dateFormat1 = @"MMM d HH:mm:ss";
		private const string _dateFormat2 = @"MMM  d HH:mm:ss";

		private static Regex _hostFormat = new Regex(@"^[\w\.]+$", RegexOptions.Compiled);
		private static Regex _msgInvalidChars = new Regex(@"[\x00-\x1F]", RegexOptions.Compiled);
		private static Regex _fullFormat = new Regex(@"^<(\d{1,3})>([A-Za-z]{3} [ \d]\d \d\d:\d\d:\d\d) (.*$)",
			RegexOptions.Compiled);
		private static Regex _priFormat = new Regex(@"^<(\d{1,3})>(.*$)",
			RegexOptions.Compiled);
		private static IFormatProvider _standardCulture = new CultureInfo("en-US");
	}
}
