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
		

		public ApiSettingsJsonFileProvider(string Filename)
		{
			var json = File.ReadAllText(Filename);
			var jsonSettings = JObject.Parse(json);

			settings = new Dictionary<string, ApiSettings>();
			foreach (var set in jsonSettings["settings"])
			{
				var method = (string)set["name"];
				var apiSettings = new ApiSettings();
				apiSettings.BatchSize = (int)set["params"]["batch_size"];
				apiSettings.ItemsMaxCount = (int)set["params"]["items_max_count"];
				apiSettings.Params = new NameValueCollection();

				foreach (JProperty p in set["params"]["request_params"])
				{
					apiSettings.Params.Add(p.Name, (string)p.Value);
				}

				settings.Add(method, apiSettings);
			}
		}

		public ApiSettings GetSettingsForMethod(string Method)
		{
			return settings[Method];
		}
	}
}
