using Collector.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Api.Settings
{
	public class ApiSettingsJsonFileProvider : IApiSettingsProvider
	{
		Dictionary<string, ApiSettings> settings;
		Tuple<string, string> listParams;

		public ApiSettingsJsonFileProvider(string Filename)
		{
			var json = File.ReadAllText(Filename);
			var jsonSettings = JObject.Parse(json);

			// Set list params
			var listOffsetParam = (string)jsonSettings["list_params"]["offset"];
			var listCountParam = (string)jsonSettings["list_params"]["count"];
			listParams = new Tuple<string, string>(listOffsetParam, listCountParam);

			// Set settings for methods
			settings = new Dictionary<string, ApiSettings>();
			foreach (var set in jsonSettings["methods"])
			{
				var method = (string)set["name"];
				var apiSettings = new ApiSettings();
				apiSettings.BatchSize = (int)set["params"]["batch_size"];
				apiSettings.ItemsMaxCount = (int)set["params"]["items_max_count"];
				apiSettings.IsNeedAccessToken = (bool)set["params"]["is_need_access_toke"];

				apiSettings.IdParams = new List<string>();
				foreach (var idParam in set["params"]["id_params"])
					apiSettings.IdParams.Add((string)idParam);
				
				apiSettings.Params = new NameValueCollection();
				if (set["params"]["request_params"] != null)
					foreach (JProperty p in set["params"]["request_params"])
						apiSettings.Params.Add(p.Name, (string)p.Value);

				settings.Add(method, apiSettings);
			}
		}

		public ApiSettings GetSettingsForMethod(string Method)
		{
			if (settings.ContainsKey(Method))
			{
				return settings[Method];
			}
			else
			{
				throw new NotSupportedException("Not supported: " + Method);
			}
		}


		public List<string> GetSupportedMethods()
		{
			return settings.Keys.ToList();
		}

		public bool IsMethodSupported(string Method)
		{
			return settings.ContainsKey(Method);
		}
		public Tuple<string, string> GetListOffsetParams()
		{
			return listParams;
		}
	}
}
