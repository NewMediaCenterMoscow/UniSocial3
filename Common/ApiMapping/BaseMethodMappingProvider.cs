using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApiMapping
{
	public abstract class BaseMethodMappingProvider : IMethodMappingProvider
	{
		Dictionary<string, string> mappings;


		public BaseMethodMappingProvider(string MappingFilename)
		{
			mappings = new Dictionary<string, string>();

			var json = File.ReadAllText(MappingFilename);
			var jsonSettings = JObject.Parse(json);

			foreach (var mp in jsonSettings["methods"])
			{
				var typeName = transfomrType((string)mp["type"]);
				mappings.Add((string)mp["name"], typeName);
			}
		}

		public Dictionary<string, string> GetMappings()
		{
			return mappings;
		}

		protected abstract string transfomrType(string Name);

	}
}
