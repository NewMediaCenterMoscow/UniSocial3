using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Api.Settings
{
	public class ApiSettings
	{
		public int BatchSize { get; set; }
		public int ItemsMaxCount { get; set; }
		public NameValueCollection Params { get; set; }
		public List<string> IdParams { get; set; }
	}
}
