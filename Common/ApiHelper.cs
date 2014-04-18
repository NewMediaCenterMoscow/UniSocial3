using Collector.Api;
using Collector.Api.Settings;
using Collector.Interface;
using Collector.Models.Vk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public class ApiHelper
	{
		Dictionary<SocialNetwork, IApi> api;
		Dictionary<string, Func<string, Task<object>>> apiCalls;

		public ApiHelper()
		{
			setApiParams();
		}

		public async Task<object> GetResult(CollectTask task)
		{
			var apiCall = getApiCall(task.SocialNetwork, task.Method);
			var result = await apiCall(task.Params);

			return result;
		}


		void setApiParams()
		{
			api = new Dictionary<SocialNetwork,IApi>();
			api.Add(SocialNetwork.VKontakte, 
				new VkApi(
					new ApiSettingsJsonFileProvider("Api/Settings/Vk.json")
				)
			);

			apiCalls = new Dictionary<string, Func<string, Task<object>>>();
			addApiCall(SocialNetwork.VKontakte, "users.get", async param => { 
					return await api[SocialNetwork.VKontakte].Get<List<VkUser>>("users.get", param.Split(',').ToList()); 
			});
		}

		void addApiCall(SocialNetwork socialNetwork, string method, Func<string, Task<object>> call)
		{
			apiCalls.Add((int)socialNetwork + method, call);
		}
		Func<string, Task<object>> getApiCall(SocialNetwork socialNetwork, string method)
		{
			var key = (int)socialNetwork + method;

			if (!apiCalls.ContainsKey(key))
			{
				throw new NotSupportedException("Not supported: " + socialNetwork + " - " + method);
			}

			return apiCalls[key];
		}
	}
}
