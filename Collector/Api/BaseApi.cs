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

		NameValueCollection getIdParam(ApiSettings settings, string id)
		{
			var param = new NameValueCollection();

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

			return param;
		}

		public async Task<T> GetObject<T>(string Method, string Id)
		{
			// Workflow:
			// 1. Create params [Ok]
			// 2. Set id param [Ok]
			// 3. Exucte request [Ok]
			// 4. Modify result

			var settings = Settings.GetSettingsForMethod(Method);

			var param = getIdParam(settings, Id);
			param.Add(settings.Params);

			var result = await ApiRequest.ExecuteRequest<T>(Method, param);

			return result;
		}
		public async Task<T> GetObject<T>(string Method, List<string> Ids)
		{
			var allIds = String.Join(",", Ids);

			return await GetObject<T>(Method, allIds);
		}

	}
}
