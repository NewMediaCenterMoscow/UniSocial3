using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Api
{
	public class VkApiRequest : BaseApiRequest
	{
		string apiVersion = "5.16";
		string accessToken = "zzz";

		NameValueCollection vkParams;

		List<string> methodNeedAuth = new List<string>() {
			"groups.get"
		};

		public VkApiRequest()
		{
			baseUri = "https://api.vk.com/method/";

			vkParams = new NameValueCollection();
			vkParams.Add("v", apiVersion);
			vkParams.Add("lang", "ru");
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
