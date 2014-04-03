using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkCityCountry
	{
		[JsonProperty("id")]
		public long Id { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
	}
}
