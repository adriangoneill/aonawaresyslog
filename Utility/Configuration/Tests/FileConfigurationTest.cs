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
	/// Tests the file configuration class
	/// </summary>
	[TestFixture] 
	public class FileConfigurationTest : ConfigurationTest
	{
		protected override Configuration CreateConfigObject()
		{
			FileConfiguration fc = new FileConfiguration(SystemName);
			fc.Initialize();
			return fc;
		}

		protected override void Initialize(Configuration cf)
		{
			FileConfiguration fc = cf as FileConfiguration;
			fc.Initialize();
		}

		[Test] public override void SaveConfig()
		{
			base.SaveConfig();
		}

		[Test] public override void LoadConfig()
		{
			base.LoadConfig();
		}

		[Test] public override void ReloadConfig()
		{
			base.ReloadConfig();
		}

		[Test] public override void InitTwice()
		{
			base.InitTwice();
		}

		[Test] public override void SaveThenLoad()
		{
			base.SaveThenLoad();
		}

		[Test] public override void ModifiedCheck()
		{
			base.ModifiedCheck();
		}
	}
}
