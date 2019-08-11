/*
Local IP Addresses
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
using System.Net;

namespace Aonaware.Utility.Networking
{
	/// <summary>
	/// Return list of all known local IP interfaces
	/// </summary>
	public class LocalIPAddresses
	{
		public static IPAddress[] AllAddresses()
		{
			try
			{
				string hostName = Dns.GetHostName();
				IPHostEntry hentry = Dns.GetHostEntry(hostName);
				
				Hashtable ht = new Hashtable(hentry.AddressList.Length);
				foreach(IPAddress addr in hentry.AddressList)
				{
					ht.Add(addr, null);
				}

				IPAddress[] ret = new IPAddress[ht.Count];
				ht.Keys.CopyTo(ret, 0);
				return ret;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
