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

			var sett = sp.GetSettingsForMethod("groups.getById");

			Assert.AreEqual(300, sett.BatchSize);
			Assert.AreEqual(1, sett.ItemsMaxCount);
			Assert.AreEqual(1, sett.IdParams.Count);
			Assert.AreEqual("group_ids", sett.IdParams[0]);
			Assert.AreEqual(1, sett.Params.Count);
			Assert.AreEqual("members_count", sett.Params["fields"]);
			Assert.AreEqual(false, sett.IsNeedAccessToken);
		}
	}
}
