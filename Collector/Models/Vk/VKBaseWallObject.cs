using Collector.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
    [Serializable]
	public class VKBaseWallObject
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("from_id")]
		public long FromId { get; set; }

		[JsonProperty("date")]
		[JsonConverter(typeof(UnixTimestampToDateTimeConverter))]
		public DateTime Date { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}
}
