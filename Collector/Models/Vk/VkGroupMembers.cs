using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkGroupMembers
	{
		public long GroupId { get; set; }

		public List<long> GroupMembers { get; set; }
	}
}
