/*
Enumerate local users and groups
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
using System.Management;

namespace Aonaware.Utility.Networking
{
	/// <summary>
	/// Enumerate all local user accounts and local groups
	/// </summary>
	public class LocalUsersAndGroups
	{
		public class UserGroupBase
		{
			protected UserGroupBase(string name, string domain, string description, string caption, string sid)
			{
				_name = name;
				_domain = domain;
				_description = description;
				_caption = caption;
				_sid = sid;
			}

			public string Name
			{
				get
				{
					return _name;
				}
			}

			public string Domain
			{
				get
				{
					return _domain;
				}
			}

			public string Description
			{
				get
				{
					return _description;
				}
			}

			public string Caption
			{
				get
				{
					return _caption;
				}
			}

			public string SID
			{
				get
				{
					return _sid;
				}
			}

			private readonly string _name;
			private readonly string _domain;
			private readonly string _description;
			private readonly string _caption;
			private readonly string _sid;
		}

		public class Group : UserGroupBase
		{
			public Group(string name, string domain, string description, string caption, string sid)
				: base(name, domain, description, caption, sid)
			{
			}
		}

		public class User : UserGroupBase
		{
			public User(string name, string domain, string fullName, string description, string caption, string sid)
				: base(name, domain, description, caption, sid)
			{
				_fullName = fullName;
			}

			public string FullName
			{
				get
				{
					return _fullName;
				}
			}

			private readonly string _fullName;
		}

		private static ManagementObjectCollection QueryWMI(string wmiClass)
		{
			ManagementScope scope = new ManagementScope("ROOT\\CIMV2"); 
			string query = @"SELECT * FROM " + wmiClass + @" WHERE Domain = """ +
				Environment.MachineName + @""""; 
			SelectQuery squery = new SelectQuery(query); 
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, squery); 
			ManagementObjectCollection queryCollection = searcher.Get();
			return queryCollection;
		}

		public static List<User> Users()
		{
            List<User> ar = new List<User>();

			ManagementObjectCollection queryCollection = QueryWMI("Win32_UserAccount");
			foreach (ManagementObject m in queryCollection) 
			{ 
				User u = new User(m["Name"].ToString(), m["Domain"].ToString(), m["FullName"].ToString(),
					m["Description"].ToString(), m["Caption"].ToString(), m["SID"].ToString());
				ar.Add(u);
			}

			return ar;
		}

        public static List<Group> Groups()
		{
            List<Group> ar = new List<Group>();

			ManagementObjectCollection queryCollection = QueryWMI("Win32_Group");
			foreach (ManagementObject m in queryCollection) 
			{ 
				Group g = new Group(m["Name"].ToString(), m["Domain"].ToString(),
					m["Description"].ToString(), m["Caption"].ToString(), m["SID"].ToString());
				ar.Add(g);
			}

			return ar;
		}
	}
}
