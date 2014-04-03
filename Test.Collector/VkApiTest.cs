using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Collector.Interface;
using Collector.Api;
using Collector.Api.Settings;
using Collector.Models.Vk;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test.Collector
{
	[TestClass]
	public class VkApiTest
	{
		[TestMethod]
		public async Task TestGetObjectOneObject()
		{
			var settProv= new ApiSettingsJsonFileProvider("Api/Settings/Vk.json");
			IApi api = new VkApi(settProv);

			var result = await api.GetObject<List<VkUser>>("users.get", "1");

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1, result[0].Id);
			Assert.AreEqual("durov", result[0].ScreenName);
		}

		[TestMethod]
		public async Task TestGetObjectMultipleObject()
		{
			var settProv = new ApiSettingsJsonFileProvider("Api/Settings/Vk.json");
			IApi api = new VkApi(settProv);

			var result = await api.GetObject<List<VkUser>>("users.get", new List<string>() { "1", "6" });

			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(1, result[0].Id);
			Assert.AreEqual("durov", result[0].ScreenName);
			Assert.AreEqual(6, result[1].Id);
			Assert.AreEqual(2, result[1].City.Id);
		}

		[TestMethod]
		public async Task TestGetObjectVkList()
		{
			var settProv = new ApiSettingsJsonFileProvider("Api/Settings/Vk.json");
			IApi api = new VkApi(settProv);

			var result = await api.GetObject<VkList<VkPost>>("wall.get", "174111803");

			Assert.AreEqual(305, result.Count);
			Assert.AreEqual(305, result.Items.Count);
			Assert.AreEqual(536, result.Items[0].Id);
			Assert.AreEqual(185202286, result.Items[0].FromId);
		}

	}
}
