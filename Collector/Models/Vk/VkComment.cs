using Collector.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkComment : VKBaseWallObject
	{
		[JsonProperty("likes")]
		public VkNestedCount Likes { get; set; }
	}
}
