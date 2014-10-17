using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Collector.Api
{
	public class VkApiRequest : BaseApiRequest
	{
		static string vkBaseUri = "https://api.vk.com/method/";

		string apiVersion = "5.21";
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
			if (Params[vkParams.AllKeys.First()] == null)
				Params.Add(vkParams);

			if (isMethodNeedAuth(Method))
				Params.Add("access_token", accessToken);

			return base.GetUri(Method, Params);
		}

		public override async Task<JToken> ExecuteRequest(string Method, NameValueCollection Params)
		{
			var res = await base.ExecuteRequest(Method, Params);

			if (res["error"] != null)
			{
				var errorCode = (int)res["error"]["error_code"];
				var errorMessage = (string)res["error"]["error_msg"];

				throw new Exception("VkEx #" + errorCode + ": " + errorMessage);
			}

			return res["response"];
		}
	}
}
