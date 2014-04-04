using Collector.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkList<T> : IApiList<VkList<T>>
	{
		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("items")]
		public List<T> Items { get; set; }

		public long GetObjectCount()
		{
			return Items.Count;
		}

		public void AppendItems(IApiList<VkList<T>> NewItems)
		{
			var newIt = NewItems.GetItems();
			Items.AddRange(newIt.Items);
		}

		public VkList<T> GetItems()
		{
			return this;
		}
	}
}
