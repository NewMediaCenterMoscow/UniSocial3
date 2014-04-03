using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Api
{
	public class VkApiRequest : BaseApiRequest
	{
		static string vkBaseUri = "https://api.vk.com/method/";

		string apiVersion = "5.16";
		string lang = "ru";

		string accessToken = "zzz";

		NameValueCollection vkParams;

		List<string> methodNeedAuth = new List<string>() {
			"groups.get"
		};

		public VkApiRequest()
			: base(VkApiRequest.vkBaseUri)
		{
			var settingsAccessToken = ConfigurationManager.AppSettings["vkAccessToken"];
			if(!String.IsNullOrEmpty(settingsAccessToken))
				accessToken = settingsAccessToken;

			vkParams = new NameValueCollection();
			vkParams.Add("v", apiVersion);
			vkParams.Add("lang", lang);
		}

		bool isMethodNeedAuth(string method)
		{
			return methodNeedAuth.Contains(method);
		}

		public override Uri GetUri(string Method, NameValueCollection Params)
		{
			Params.Add(vkParams);

			if (isMethodNeedAuth(Method))
				Params.Add("access_token", accessToken);

			return base.GetUri(Method, Params);
		}
	}
}
