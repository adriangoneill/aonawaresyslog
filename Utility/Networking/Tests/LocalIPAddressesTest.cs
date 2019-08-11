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

namespace Aonaware.Utility.Networking.Tests
{
	/// <summary>
	/// Tests the LocalIPAddresses class.
	/// </summary>
	[TestFixture]
	public class LocalIPAddressesTest
	{
		[Test] public void LocalIP()
		{
			IPAddress[] addresses = LocalIPAddresses.AllAddresses();
			
			Assert.IsTrue(addresses != null, "No IP addresses returned");
			Assert.IsTrue(addresses.Length > 0, "No IP addresses found");
		}
	}
}
