using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Collector.Api.Settings;

namespace Test.Collector
{
	[TestClass]
	public class ApiSettingsJsonFileProviderTest
	{
		[TestMethod]
		public void TestLoadSettings()
		{
			var sp = new ApiSettingsJsonFileProvider("Api/Settings/Vk.json");
		}

		[TestMethod]
		public void TestGetSettingsForMethod()
		{
			var sp = new ApiSettingsJsonFileProvider("Api/Settings/Vk.json");

			var sett = sp.GetSettingsForMethod("test_method");

			Assert.AreEqual(1, sett.BatchSize);
			Assert.AreEqual(200, sett.ItemsMaxCount);
			Assert.AreEqual(sett.Params["test_param1"], "test_val1");
		}
	}
}
