using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Interface
{
	public interface IApi
	{
		Task<T> Get<T>(string Method, string Id);
		Task<T> Get<T>(string Method, List<string> Ids);

		IApiSettingsProvider Settings { get; }
		IApiRequest ApiRequest { get; }
	}
}
