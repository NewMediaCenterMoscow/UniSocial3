using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkUserGroups
	{
		public long UserId { get; set; }

		public List<long> Groups { get; set; }
	}
}
