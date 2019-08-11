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
using System.Collections;

using NUnit.Framework;

namespace Aonaware.Utility.Configuration.Tests
{
	/// <summary>
	/// Shared class to test configuration
	/// </summary>
	public abstract class ConfigurationTest
	{
		protected ConfigurationTest()
		{
			_values = new Hashtable();
			_values["String Value"] = "Text";
			_values["Integer Value"] = 49;
			_values["Float Value"] = 3.141;
			_values["Boolean Value"] = false;
		}

		protected void PopulateConfig(Configuration cf)
		{
			IDictionaryEnumerator en = _values.GetEnumerator();
			while (en.MoveNext())
			{
				cf.SetValue((string) en.Key, en.Value);
			}
		}

		protected void CheckConfig(Configuration cf)
		{
			IDictionaryEnumerator en = _values.GetEnumerator();
			while (en.MoveNext())
			{
				string key = (string) en.Key;
				Assert.AreEqual(en.Value, cf.GetValue(key), key);
				Assert.AreEqual(en.Value.GetType(), cf.GetValue(key).GetType(), key + " type");
			}
		}

		protected abstract Configuration CreateConfigObject();
		protected abstract void Initialize(Configuration cf);

		protected Configuration StoreDefaultConfig()
		{
			Configuration cf = CreateConfigObject();
			PopulateConfig(cf);
			cf.StoreConfiguration();
			return cf;
		}

		public virtual void SaveConfig()
		{
			Configuration cf = StoreDefaultConfig();
			CheckConfig(cf);
		}

		public virtual void LoadConfig()
		{
			StoreDefaultConfig();

			Configuration cf = CreateConfigObject();
			CheckConfig(cf);
		}

		public virtual void ReloadConfig()
		{
			StoreDefaultConfig();

			Configuration cf = CreateConfigObject();
			cf.ReloadConfiguration();
			CheckConfig(cf);
		}

		public virtual void InitTwice()
		{
			StoreDefaultConfig();

			Configuration cf = CreateConfigObject();
			Initialize(cf);
			CheckConfig(cf);
		}

		public virtual void SaveThenLoad()
		{
			Configuration cf = StoreDefaultConfig();
			Initialize(cf);
			CheckConfig(cf);
		}

		public virtual void ModifiedCheck()
		{
			Configuration cf = StoreDefaultConfig();
			Assert.IsFalse(cf.Modified, "Default value should not be modified");

			cf.SetValue("Testing", "Testing");
			cf.SetValue("Testing", "Testing2");
			Assert.IsTrue(cf.Modified, "Should be modified if I change it");

			cf.StoreConfiguration();
			Assert.IsFalse(cf.Modified, "Saving should clear modified flag");

			cf.SetValue("Testing", "Testing2");
			Assert.IsFalse(cf.Modified, "Setting to same value should not change modified flag");
		}

		protected const string SystemName = "Testing";
		private Hashtable _values;
	}
}
