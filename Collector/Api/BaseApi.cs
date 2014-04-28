using Collector.Api.Settings;
using Collector.Common;
using Collector.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Collector.Api
{
	public abstract class BaseApi : IApi
	{
		public IApiRequest ApiRequest { get; protected set; }
		public IApiSettingsProvider Settings { get; protected set; }

		public BaseApi(IApiRequest ApiRequest, IApiSettingsProvider ApiSettingsProvider)
		{
			this.ApiRequest = ApiRequest;
			this.Settings = ApiSettingsProvider;
		}

		protected void setListParams(NameValueCollection param, long offset, long count)
		{
			var listParam = Settings.GetListOffsetParams();

			param.Set(listParam.Item1, offset.ToString());
			param.Set(listParam.Item2, count.ToString());
		}
		protected void setIdParam(NameValueCollection param, ApiSettings settings, string id)
		{
			if (settings.IdParams.Count == 1)
			{
				param.Add(settings.IdParams[0], id);
			}
			else
			{
				var ids = id.Split('_');
				for (int i = 0; i < settings.IdParams.Count; i++)
				{
					param.Add(settings.IdParams[i], ids[i]);
				}
			}
		}

		//protected bool isNeedList(Type t)
		protected bool isNeedList(ApiSettings settings)
		{
			//var apiListType = typeof(IApiList<>);

			//var result = 
			//	t.GetInterfaces().Any(
			//		it => it.IsGenericType && it.GetGenericTypeDefinition() == apiListType
			//	);
			
			//return result;

			return settings.ItemsMaxCount != 1;
		}

		protected async Task<T> getList<T>(string Method, NameValueCollection param, ApiSettings settings)
		{
			var maxCount = settings.ItemsMaxCount;

			var currentOffset = 0L;
			var currentCount = 0L;
			T resultList = default(T);

			while (true)
			{
				setListParams(param, currentOffset, maxCount);

				var curentResult = await ApiRequest.ExecuteRequest<T>(Method, param);

				if (curentResult != null)
				{
					if (resultList == null)
					{
						resultList = curentResult;
					}
					else
					{
						(resultList as IApiList<T>).AppendItems(curentResult as IApiList<T>);
					}

					currentCount = (curentResult as IApiList<T>).GetObjectCount();

					if (currentCount < maxCount)
					{
						break;
					}

					currentOffset += currentCount;
				}
				else
				{
					break;
				}
			}

			return resultList;
		}

		public async Task<T> Get<T>(string Method, string Id)
		{
			var settings = Settings.GetSettingsForMethod(Method);

			var param = new NameValueCollection();

			setIdParam(param, settings, Id);
			param.Add(settings.Params);

			T result = default(T);

			if (isNeedList(settings))
			{
				result = await getList<T>(Method, param, settings);
			}
			else
			{
				result = await ApiRequest.ExecuteRequest<T>(Method, param);
			}

			return result;
		}

		public async Task<T> Get<T>(string Method, List<string> Ids)
		{
			var allIds = String.Join(",", Ids);

			return await Get<T>(Method, allIds);
		}
	}
}
