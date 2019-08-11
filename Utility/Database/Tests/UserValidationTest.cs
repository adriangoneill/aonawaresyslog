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
using System.Configuration;

using NUnit.Framework;

namespace Aonaware.Utility.Database.Tests
{
	/// <summary>
	/// Tests the UserValidation class
	/// </summary>
	[TestFixture]
	public class UserValidationTest
	{
		[TestFixtureSetUp] public void Initialize()
		{
            string db = Properties.Settings.Default.DbConnection;
            if (db == null || db.Length == 0)
			{
				Console.WriteLine("ConnectionString not found in app config, tests will fail!");
			}

			// Initialize
			UserValidation.Instance.Initialize(db);

			// Create test user
			UserValidation.Instance.AddUser(_user, _pass);
		}

		[TestFixtureTearDown] public void Dispose()
		{
			// Remove test user
			UserValidation.Instance.DeleteUser(_user);
		}

		[Test] public void CreateDeleteUser()
		{
			string user = Guid.NewGuid().ToString();
			string pass = Guid.NewGuid().ToString();

			UserValidation.Instance.AddUser(user, pass);
			Assert.IsTrue(UserValidation.Instance.ValidateUser(user, pass));

			UserValidation.Instance.DeleteUser(user);
			Assert.IsFalse(UserValidation.Instance.ValidateUser(user, pass));
		}

		[Test] public void CorrectPassword()
		{
			Assert.IsTrue(UserValidation.Instance.ValidateUser(_user, _pass));
		}

		[Test] public void IncorrectPassword()
		{
			Assert.IsFalse(UserValidation.Instance.ValidateUser(_user, "Incorrect"));
		}

		private const string _user = "TestUser";
		private const string _pass = "TestPassword";
	}
}
