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
using System.Text.RegularExpressions;
using System.Collections.Generic;

using NUnit.Framework;

namespace Aonaware.Utility.Networking.Tests
{
	/// <summary>
	/// Test the LocalUsersAndGroups class.
	/// </summary>
	[TestFixture]
	public class LocalUsersAndGroupsTest
	{
		[Test] public void TestUsers()
		{
            List<LocalUsersAndGroups.User> users = LocalUsersAndGroups.Users();

			// Find administrator
			bool found = false;
			foreach (LocalUsersAndGroups.User u in users)
			{
				if (Regex.IsMatch(u.SID, "^S-1-5-.*-500$"))
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue(found, "Administrator user not found");
		}

		[Test] public void TestGroups()
		{
            List<LocalUsersAndGroups.Group> groups = LocalUsersAndGroups.Groups();

			// Find administrators
			bool found = false;
			foreach (LocalUsersAndGroups.Group g in groups)
			{
				if (g.SID == "S-1-5-32-544")
				{
					found = true;
					break;
				}
			}

			Assert.IsTrue(found, "Administrators group not found");
		}
	}
}
