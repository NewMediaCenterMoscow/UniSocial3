using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public enum VkGroupType
	{
		Group,
		Page,
		Event
	}
	public class VkGroup
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("screen_name")]
		public string ScreenName { get; set; }

		[JsonProperty("is_closed")]
		public bool IsClosed { get; set; }
		
		[JsonProperty("type")]
		public VkGroupType Type { get; set; }

		[JsonProperty("members_count")]
		public int MembersCount { get; set; }

		//[JsonProperty("data_collection_id")]
		//public int DataCollectionId { get; set; }
 	}
}
