using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    [Serializable]
	public class CollectTaskResult
	{
		public CollectTask Task { get; set; }
		public object Result { get; set; }
	}
}
