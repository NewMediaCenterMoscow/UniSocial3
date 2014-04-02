using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Common
{
	class VkUserDeactivatedConverted : JsonConverter 
	{

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(string);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = (string)reader.Value;

			if (value == "deleted" || value == "banned")
				return true;
			else
				return false;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
