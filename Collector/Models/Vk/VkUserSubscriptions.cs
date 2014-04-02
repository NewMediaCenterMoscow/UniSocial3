using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkUserSubscriptions
	{
		public long Id { get; set; }

		[JsonProperty("users")]
		public VkList<long> Users { get; set; }

		[JsonProperty("groups")]
		public VkList<long> Groups { get; set; }
	}
}
