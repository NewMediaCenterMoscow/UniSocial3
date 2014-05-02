using Collector.Api;
using Collector.Api.Settings;
using Collector.Interface;
using Collector.Models.Vk;
using Common.ApiMapping;
using Common.Model;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public class ApiHelper
	{
		Dictionary<string, Func<string, object>> apiCalls;
		Dictionary<string, Func<string, object>> deserializationCalls;

		static Dictionary<SocialNetwork, IKernel> ninjectKernels;

		static ApiHelper()
		{
			ninjectKernels = new Dictionary<SocialNetwork, IKernel>();
			ninjectKernels.Add(SocialNetwork.VKontakte, new StandardKernel(new NinjectModules.Vk()));
		}

		public ApiHelper()
		{
			setCallParams();
		}

		public object GetResult(CollectTask task)
		{
			var apiCall = getCall(apiCalls, task.SocialNetwork, task.Method);
			var result = apiCall(task.Params);

			return result;
		}

		public object DeserializeResult(CollectTask task, string result)
		{
			var desCall = getCall(deserializationCalls, task.SocialNetwork, task.Method);
			var res = desCall(result);

			return res;
		}

		void setCallParams()
		{
			apiCalls = new Dictionary<string, Func<string, object>>();
			deserializationCalls = new Dictionary<string, Func<string, object>>();

			setCallParams(SocialNetwork.VKontakte);
		}

		void setCallParams(SocialNetwork socialNetwork)
		{
			var api = ninjectKernels[socialNetwork].Get<IApi>();
			var mappings = ninjectKernels[socialNetwork].Get<IMethodMappingProvider>().GetMappings();
			var apiSettingsProvider = ninjectKernels[socialNetwork].Get<IApiSettingsProvider>();

			var apiObjType = typeof(ApiHelper);
			var desObjType = typeof(JsonConvert);

			//var desMethod = desObjType.GetMethod("DeserializeObject", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder, new Type[] { typeof(string) }, null);
			var desMethod = desObjType.GetMethods().Where(m => m.Name == "DeserializeObject" && m.IsGenericMethod && m.GetParameters().Count() == 1).First();

			// Key - method name, value - type
			foreach (var mp in mappings)
			{
				var methodSettings = apiSettingsProvider.GetSettingsForMethod(mp.Key);
				var needType = Type.GetType(mp.Value);

				Type [] apiParamsType = null;
				if (methodSettings.BatchSize == 1)
					apiParamsType = new Type[] { typeof(IApi), typeof(string), typeof(string) };
				else
					apiParamsType = new Type[] { typeof(IApi), typeof(string), typeof(List<string>) };
				var apiMethod = apiObjType.GetMethod("getApiResult", BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, apiParamsType, null);

				var apiGeneric = apiMethod.MakeGenericMethod(needType);
				if (methodSettings.BatchSize == 1)
					addCall(apiCalls, socialNetwork, mp.Key, param => apiGeneric.Invoke(this, new object[] { api, mp.Key, param }));
				else
					addCall(apiCalls, socialNetwork, mp.Key, param => apiGeneric.Invoke(this, new object[] { api, mp.Key, param.Split(',').ToList() }));


				var desGeneric = desMethod.MakeGenericMethod(needType);
				addCall(deserializationCalls, socialNetwork, mp.Key, param => desGeneric.Invoke(this, new object[] { param }));

			}

		}

		void addCall(Dictionary<string, Func<string, object>> callStorage, SocialNetwork socialNetwork, string method, Func<string, object> call)
		{
			var key = (int)socialNetwork + method;
			callStorage.Add(key, call);
		}
		Func<string, object> getCall(Dictionary<string, Func<string, object>> callStorage, SocialNetwork socialNetwork, string method)
		{
			var key = (int)socialNetwork + method;

			if (!callStorage.ContainsKey(key))
				throw new NotSupportedException("Not supported: " + socialNetwork + " - " + method);

			return callStorage[key];
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
