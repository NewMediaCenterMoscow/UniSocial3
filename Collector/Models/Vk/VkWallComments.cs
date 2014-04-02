using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public class VkWallComments
	{
		public long OwnerId { get; set; }
		public long PostId { get; set; }

		public List<VkComment> Comments { get; set; }
	}
}
