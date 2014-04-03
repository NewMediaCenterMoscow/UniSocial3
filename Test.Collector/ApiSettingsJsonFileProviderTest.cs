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
			Assert.AreEqual(1, sett.IdParams.Count);
			Assert.AreEqual("id", sett.IdParams[0]);
			Assert.AreEqual(2, sett.Params.Count);
			Assert.AreEqual("test_val1", sett.Params["test_param1"]);
		}
	}
}
