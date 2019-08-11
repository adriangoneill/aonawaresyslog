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

using NUnit.Framework;

namespace Aonaware.Syslog.Tests
{
	/// <summary>
	/// Tests the SyslogMessageTest class
	/// </summary>
	[TestFixture]
	public class SyslogMessageTest
	{
		[Test] public void Construction()
		{
			string host = "localhost";
			string message = "test message!";

			SyslogMessage m = new SyslogMessage(host, message);
			Assert.AreEqual(m.LocalHost, host, "Hostnames differ");
			Assert.AreEqual(m.Message, message, "Messages differ");
			Assert.AreEqual(SyslogMessage.SeverityCode.Notice, m.Severity, 
				"Default severity incorrect");
			Assert.AreEqual(SyslogMessage.FacilityCode.UserLevel, m.Facility, 
				"Default facility incorrect");
		}

		[Test] public void MessageLength()
		{
			string host = "localhost";
			string message = new string('x', 2048);
			SyslogMessage m = new SyslogMessage(host, message);
			Assert.AreEqual(1024, m.ToString().Length, "Max message Length");
		}

		[Test] public void StringConversion()
		{
			string host = "localhost";
			string message = "test message!";
			DateTime time1 = new DateTime(2000,12,1,16,12,15,0);
			SyslogMessage m1 = new SyslogMessage(host, message, SyslogMessage.FacilityCode.UserLevel,
				SyslogMessage.SeverityCode.Notice, time1);
			Assert.AreEqual("<13>Dec  1 16:12:15 localhost test message!", m1.ToString(),
				"String conversion");

			DateTime time2 = new DateTime(2000,12,25,16,12,15,0);
			SyslogMessage m2 = new SyslogMessage(host, message, SyslogMessage.FacilityCode.UserLevel,
				SyslogMessage.SeverityCode.Notice, time2);
			Assert.AreEqual("<13>Dec 25 16:12:15 localhost test message!", m2.ToString(),
				"String conversion");
		}
		
		[Test] public void Parsing()
		{
			IPAddress ip = IPAddress.Loopback;

			SyslogMessage m1 = SyslogMessage.Parse(ip,
				"<13>Dec  1 16:12:15 localhost test message!");
			Assert.AreEqual(SyslogMessage.FacilityCode.UserLevel, m1.Facility,
				"Facility Code Parsing");
			Assert.AreEqual(SyslogMessage.SeverityCode.Notice, m1.Severity,
				"Severity Code Parsing");
			Assert.AreEqual(ip.ToString(), m1.LocalHost, "Host parsing");
			Assert.AreEqual("localhost test message!", m1.Message, "Message parsing");
			Assert.AreEqual(12, m1.LocalTime.Month, "Message month");
			Assert.AreEqual(16, m1.LocalTime.Hour, "Message hour");

			SyslogMessage m2 = SyslogMessage.Parse(ip,
				"<34>Oct 11 22:14:15 mymachine su: 'su root' failed for lonvick on /dev/pts/8");
			Assert.AreEqual(SyslogMessage.FacilityCode.Security1, m2.Facility,
				"Facility Code Parsing");
			Assert.AreEqual(SyslogMessage.SeverityCode.Critical, m2.Severity,
				"Severity Code Parsing");
			Assert.AreEqual(ip.ToString(), m2.LocalHost, "Host parsing");
			Assert.AreEqual("mymachine su: 'su root' failed for lonvick on /dev/pts/8",
				m2.Message, "Message parsing");
			Assert.AreEqual(10, m2.LocalTime.Month, "Message month");
			Assert.AreEqual(22, m2.LocalTime.Hour, "Message hour");

			SyslogMessage m3 = SyslogMessage.Parse(ip, "No header");
			Assert.AreEqual(SyslogMessage.FacilityCode.UserLevel, m3.Facility,
				"Facility Code Parsing");
			Assert.AreEqual(SyslogMessage.SeverityCode.Notice, m3.Severity,
				"Severity Code Parsing");
			Assert.AreEqual("No header", m3.Message, "Message parsing");

			SyslogMessage m4 = SyslogMessage.Parse(ip, "<999>Invalid header");
			Assert.AreEqual("<999>Invalid header", m4.Message, "Message parsing");

			SyslogMessage m5 = SyslogMessage.Parse(ip, "<1>Invalid date");
			Assert.AreEqual(SyslogMessage.FacilityCode.Kernel, m5.Facility,
				"Facility Code Parsing");
			Assert.AreEqual(SyslogMessage.SeverityCode.Alert, m5.Severity,
				"Severity Code Parsing");
			Assert.AreEqual("Invalid date", m5.Message, "Message parsing");
		}
	}
}
