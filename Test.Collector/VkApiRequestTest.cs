using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Collector.Interface;
using Collector.Api;
using System.Collections.Specialized;

namespace Test.Collector
{
	[TestClass]
	public class VkApiRequestTest
	{
		[TestMethod]
		public void TestGetUri()
		{
			IApiRequest req = new VkApiRequest();

			var need = "https://api.vk.com/method/test?v=5.16&lang=ru";
			var result = req.GetUri("test").ToString();
			Assert.AreEqual(need, result);
		}
		[TestMethod]
		public void TestGetUriTrimStartSlash()
		{
			IApiRequest req = new VkApiRequest();

			var need = "https://api.vk.com/method/test?v=5.16&lang=ru";
			var result = req.GetUri("/test").ToString();
			Assert.AreEqual(need, result);
		}
		[TestMethod]
		public void TestGetUriTrimEndSlash()
		{
			IApiRequest req = new VkApiRequest();

			var need = "https://api.vk.com/method/test?v=5.16&lang=ru";
			var result = req.GetUri("test/").ToString();
			Assert.AreEqual(need, result);
		}
		[TestMethod]
		public void TestGetUriParam()
		{
			IApiRequest req = new VkApiRequest();

			var param = new NameValueCollection();
			param.Add("id", "1");

			var result = req.GetUri("test", param).ToString();
			var need = "https://api.vk.com/method/test?id=1&v=5.16&lang=ru";
			Assert.AreEqual(need, result);
		}
		[TestMethod]
		public void TestGetUriParams()
		{
			IApiRequest req = new VkApiRequest();

			var param = new NameValueCollection();
			param.Add("p1", "3");
			param.Add("p2", "7");

			var result = req.GetUri("test", param).ToString();
			var need = "https://api.vk.com/method/test?p1=3&p2=7&v=5.16&lang=ru";
			Assert.AreEqual(need, result);
		}
		[TestMethod]
		public void TestGetUriAppendAccessToken()
		{
			IApiRequest req = new VkApiRequest();

			var result = req.GetUri("groups.get").ToString();
			var need = "https://api.vk.com/method/groups.get?v=5.16&lang=ru&access_token=zzz";
			Assert.AreEqual(need, result);
		}
	}
}
