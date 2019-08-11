/*
User Validation
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
using System.Data;
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace Aonaware.Utility.Database
{
	/// <summary>
	/// Validate users via database
	/// </summary>
	public sealed class UserValidation
	{
		// Singleton
		public static UserValidation Instance
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

			internal static readonly UserValidation instance = new UserValidation();
		}

		private UserValidation()
		{
		}

		public void Initialize(string connString)
		{
			conn = new OleDbConnection(connString);
		}

		public static string HashPassword(string pass)
		{
			// Compute MD5 hash of password and return base64 encoding
			MD5 md5 = new MD5CryptoServiceProvider();
			ASCIIEncoding ascii = new ASCIIEncoding();
			byte[] result = md5.ComputeHash(ascii.GetBytes(pass));
			return Convert.ToBase64String(result);
		}

		private OleDbCommand CreateCommand(string sql)
		{
			// Open connection if needed
			if (conn.State != ConnectionState.Open)
				conn.Open();

			return new OleDbCommand(sql, conn);
		}

		public bool ValidateUser(string user, string pass)
		{
			lock (this)
			{
				if (conn == null)
					throw new Exception("User Validation database connection not set");

				user = user.ToLower().Trim();
				pass = HashPassword(pass);

				const string sql = "SELECT COUNT(*) FROM users WHERE userName = ? " +
						  "AND password = ? AND active = 1";
				OleDbCommand cmd = CreateCommand(sql);
				cmd.Parameters.Add("@userName", OleDbType.Char, 16, "userName");
				cmd.Parameters.Add("@password", OleDbType.Char, 32, "password");
				cmd.Parameters[0].Value = user;
				cmd.Parameters[1].Value = pass;

				int count = Convert.ToInt32(cmd.ExecuteScalar());

				// Give connection back
				conn.Close();

				bool valid = (count == 1);

				if (!valid && valSwitch.TraceWarning)
					Debug.WriteLine("Invalid login attempt, user=" + user, DbTraceListener.catWarn);

				if (valid && valSwitch.TraceInfo)
					Debug.WriteLine("Authentication success, user=" + user, DbTraceListener.catInfo);

				return valid;
			}
		}

		public bool ChangePassword(string user, string oldPassword, string newPassword)
		{
			lock (this)
			{
				if (!ValidateUser(user, oldPassword))
					return false;

				if (newPassword.Length == 0)
					throw new Exception("Password cannot be blank");

				user = user.ToLower().Trim();
				newPassword = HashPassword(newPassword);

				const string sql = "UPDATE users SET password = ? WHERE userName = ?";
				OleDbCommand cmd = CreateCommand(sql);
				cmd.Parameters.Add("@password", OleDbType.Char, 32, "password");
				cmd.Parameters.Add("@userName", OleDbType.Char, 16, "userName");
				cmd.Parameters[0].Value = newPassword;
				cmd.Parameters[1].Value = user;
			
				int rowCount = cmd.ExecuteNonQuery();

				// Give connection back
				conn.Close();

				bool valid = (rowCount == 1);

				if (!valid && valSwitch.TraceWarning)
					Debug.WriteLine("Unable to change password, user=" + user, DbTraceListener.catWarn);

				if (valid && valSwitch.TraceInfo)
					Debug.WriteLine("Password successfully changed, user=" + user, DbTraceListener.catInfo);

				return valid;
			}
		}

		public void AddUser(string user, string password)
		{
			lock (this)
			{
				if (password.Length == 0)
					throw new Exception("Password cannot be blank");

				user = user.ToLower().Trim();
				password = HashPassword(password);

				const string sql = "INSERT INTO users (userName, password, active) " +
						  "VALUES (?, ?, 1)";
				OleDbCommand cmd = CreateCommand(sql);
				cmd.Parameters.Add("@userName", OleDbType.Char, 16, "userName");
				cmd.Parameters.Add("@password", OleDbType.Char, 32, "password");
				cmd.Parameters[0].Value = user;
				cmd.Parameters[1].Value = password;

				cmd.ExecuteNonQuery();

				if (valSwitch.TraceInfo)
					Debug.WriteLine("New user created, user=" + user, DbTraceListener.catInfo);
			}
		}

		public void DeleteUser(string user)
		{
			lock (this)
			{
				user = user.ToLower().Trim();

				const string sql = "DELETE FROM users WHERE userName = ? ";
				OleDbCommand cmd = CreateCommand(sql);
				cmd.Parameters.Add("@userName", OleDbType.Char, 16, "userName");
				cmd.Parameters[0].Value = user;

				cmd.ExecuteNonQuery();

				if (valSwitch.TraceInfo)
					Debug.WriteLine("User deleted, user=" + user, DbTraceListener.catInfo);
			}
		}

		static private TraceSwitch valSwitch = new TraceSwitch("UserValidation", "User validation trace level");

		private OleDbConnection conn = null;
	}
}
