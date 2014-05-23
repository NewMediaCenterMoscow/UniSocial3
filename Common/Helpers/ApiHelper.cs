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
using System.Diagnostics;
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

		public static string GetKey(SocialNetwork Network, string Method)
		{
			return (int)Network + Method;
		}

		void setCallParams()
		{
			apiCalls = new Dictionary<string, Func<string, object>>();

			setCallParams(SocialNetwork.VKontakte);
		}

		void setCallParams(SocialNetwork socialNetwork)
		{
			var api = ninjectKernels[socialNetwork].Get<IApi>();
			var mappings = ninjectKernels[socialNetwork].Get<IMethodMappingProvider>().GetTypeMappings();
			var apiSettingsProvider = ninjectKernels[socialNetwork].Get<IApiSettingsProvider>();

			var apiObjType = typeof(ApiHelper);

			// Key - method name, value - type
			foreach (var mp in mappings)
			{
				var methodSettings = apiSettingsProvider.GetSettingsForMethod(mp.Key);
				var needType = Type.GetType(mp.Value);

				Type[] apiParamsType = null;
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

			}

		}

		void addCall(Dictionary<string, Func<string, object>> callStorage, SocialNetwork socialNetwork, string method, Func<string, object> call)
		{
			var key = GetKey(socialNetwork, method);
			callStorage.Add(key, call);
		}
		Func<string, object> getCall(Dictionary<string, Func<string, object>> callStorage, SocialNetwork socialNetwork, string method)
		{
			var key = GetKey(socialNetwork, method);

			if (!callStorage.ContainsKey(key))
				throw new NotSupportedException("Not supported: " + socialNetwork + " - " + method);

			return callStorage[key];
		}

		protected object getApiResult<T>(IApi api, string method, string param)
		{
			try
			{
				var task = api.Get<T>(method, param);
				task.Wait();
				return task.Result;

			}
			catch (AggregateException ex)
			{
				ex.Handle(x => { Trace.TraceError(x.Message); return true; });
				return null;
			}
		}
		protected object getApiResult<T>(IApi api, string method, List<string> param)
		{
			try
			{
				var task = api.Get<T>(method, param);
				task.Wait();
				return task.Result;
			}
			catch (AggregateException ex)
			{
				ex.Handle(x => { Trace.TraceError(x.Message); return true; });
				return null;
			}
		}
	}
}
