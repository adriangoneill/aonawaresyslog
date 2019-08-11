/*
IP Verification
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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace Aonaware.Utility.Networking
{
	/// <summary>
	/// Checks IP against list of hosts / regexs
	/// </summary>
	public class IPCheck
	{
		public IPCheck(List<string> ar)
		{
            _IPRegexList = new List<Regex>();
            _IPHostHash = new Dictionary<IPAddress, bool>();
			_historyHash = new Dictionary<IPAddress, bool>();

			foreach (string s in ar)
			{
				// Determine if host or regex
				try
				{
					IPAddress ip = IPAddress.Parse(s);
					_IPHostHash.Add(ip, true);
				}
				catch (Exception)
				{
					_IPRegexList.Add(new Regex(s, RegexOptions.Compiled));
				}
			}
		}

		public bool Check(IPAddress ip)
		{
            bool result;

			lock (this)
			{
				if (_historyHash.TryGetValue(ip, out result))
				    return result;
			
			    result = DoCheck(ip);
				_historyHash[ip] = result;
			}
			return result;
		}

		private bool DoCheck(IPAddress ip)
		{
			if (_IPHostHash.ContainsKey(ip))
				return true;

			string ips = ip.ToString();
			foreach (Regex r in _IPRegexList)
			{
				if (r.IsMatch(ips))
					return true;
			}

			return false;
		}

        private List<Regex> _IPRegexList;
		private Dictionary<IPAddress, bool> _IPHostHash;
        private Dictionary<IPAddress, bool> _historyHash;
	}
}
