using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApiMapping
{
	public abstract class BaseMethodMappingProvider : IMethodMappingProvider
	{
		Dictionary<string, string> typeMappings;


		public BaseMethodMappingProvider(string MappingFilename)
		{
			typeMappings = new Dictionary<string, string>();

			var json = File.ReadAllText(MappingFilename);
			var jsonSettings = JObject.Parse(json);

			foreach (var mp in jsonSettings["methods"])
			{
				var typeName = transfomrType((string)mp["type"]);
				typeMappings.Add((string)mp["name"], typeName);
			}
		}

		public Dictionary<string, string> GetTypeMappings()
		{
			return typeMappings;
		}

		public string GetTypeMapping(string Method)
		{
			return typeMappings[Method];
		}

		protected abstract string transfomrType(string Name);
	}
}
