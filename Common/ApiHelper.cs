using Collector.Api;
using Collector.Api.Settings;
using Collector.Interface;
using Collector.Models.Vk;
using Common.ApiMapping;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApiHelper
{
	public class ApiHelper
	{
		Dictionary<string, Func<string, object>> apiCalls;

		static Dictionary<SocialNetwork, IKernel> ninjectKernels;

		static ApiHelper()
		{
			ninjectKernels = new Dictionary<SocialNetwork, IKernel>();
			ninjectKernels.Add(SocialNetwork.VKontakte, new StandardKernel(new NinjectModules.Vk()));
		}

		public ApiHelper()
		{
			setApiParams();
		}

		//public async Task<object> GetResult(CollectTask task)
		//{
		//	var apiCall = getApiCall(task.SocialNetwork, task.Method);
		//	var result = await apiCall(task.Params);

		//	return result;
		//}
		public object GetResult(CollectTask task)
		{
			var apiCall = getApiCall(task.SocialNetwork, task.Method);
			var result = apiCall(task.Params);

			return result;
		}


		void setApiParams()
		{
			apiCalls = new Dictionary<string, Func<string, object>>();

			//// Vk
			//var apiVk = ninjectKernels[SocialNetwork.VKontakte].Get<IApi>();
			//addApiCall(SocialNetwork.VKontakte, "groups.getById", async param =>
			//{
			//	return await apiVk.Get<List<VkGroup>>("groups.getById", param.Split(',').ToList());
			//});
			//addApiCall(SocialNetwork.VKontakte, "groups.getMembers", async param =>
			//{
			//	return await apiVk.Get<VkList<long>>("groups.getMembers", param);
			//});
			//addApiCall(SocialNetwork.VKontakte, "users.get", async param =>
			//{
			//	return await apiVk.Get<List<VkUser>>("users.get", param.Split(',').ToList());
			//});

			setApiParams(SocialNetwork.VKontakte);
		}

		void setApiParams(SocialNetwork socialNetwork)
		{
			var api = ninjectKernels[socialNetwork].Get<IApi>();
			var mappings = ninjectKernels[socialNetwork].Get<IMethodMappingProvider>().GetMappings();
			var apiSettingsProvider = ninjectKernels[socialNetwork].Get<IApiSettingsProvider>();

			// Key - method name, value - type
			foreach (var mp in mappings)
			{
				var methodSettings = apiSettingsProvider.GetSettingsForMethod(mp.Key);

				var objType = this.GetType();//api.GetType();
				var needType = Type.GetType(mp.Value);

				Type [] paramsType = null;
				if (methodSettings.BatchSize == 1)
					paramsType = new Type[] { typeof(IApi), typeof(string), typeof(string) };
				else
					paramsType = new Type[] { typeof(IApi), typeof(string), typeof(List<string>) };

				var method = objType.GetMethod("getApiResult", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, paramsType, null);
				var generic = method.MakeGenericMethod(needType);

				if (methodSettings.BatchSize == 1)
					addApiCall(socialNetwork, mp.Key, param => generic.Invoke(this, new object[] { api, mp.Key, param }));
				else
					addApiCall(socialNetwork, mp.Key, param => generic.Invoke(this, new object[] { api, mp.Key, param.Split(',').ToList() }));
			}

		}

		void addApiCall(SocialNetwork socialNetwork, string method, Func<string, object> call)
		{
			var key = (int)socialNetwork + method;
			apiCalls.Add(key, call);
		}
		Func<string, object> getApiCall(SocialNetwork socialNetwork, string method)
		{
			var key = (int)socialNetwork + method;

			if (!apiCalls.ContainsKey(key))
				throw new NotSupportedException("Not supported: " + socialNetwork + " - " + method);

			return apiCalls[key];
		}

		protected object getApiResult<T>(IApi api, string method, string param)
		{
			var task = api.Get<T>(method, param);
			task.Wait();
			return task.Result;
		}
		protected object getApiResult<T>(IApi api, string method, List<string> param)
		{
			var task = api.Get<T>(method, param);
			task.Wait();
			return task.Result;
		}
	}
}
