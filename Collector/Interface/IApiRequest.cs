using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Interface
{
	public interface IApiRequest
	{
		void SetBaseUri(string BaseUri);

		Task<JObject> ExecuteRequest(string Method);
		Task<JObject> ExecuteRequest(string Method, NameValueCollection Params);
		Task<T> ExecuteRequest<T>(string Method);
		Task<T> ExecuteRequest<T>(string Method, NameValueCollection Params);
	}
}
