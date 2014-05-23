using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
    [Serializable]
	public class VkUserSubscriptions
	{
		[JsonProperty("users")]
		public VkList<long> Users { get; set; }

		[JsonProperty("groups")]
		public VkList<long> Groups { get; set; }
	}
}
