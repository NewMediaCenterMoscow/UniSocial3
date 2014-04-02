using Collector.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Collector.Api
{
	public abstract class BaseApiRequest : IApiRequest
	{
		protected HttpClient client = new HttpClient();
		protected string baseUri;

		protected static int requestNumber = 0;

		protected static int maxRepeats = 10;
		protected static int baseRepeatInterval = 6000;

		protected virtual Uri getUri(string baseUri, NameValueCollection Params)
		{
			var builder = new UriBuilder(baseUri);

			if (Params != null)
				builder.Query = String.Join("&", Params.AllKeys.Select(k => k + "=" + Params[k]));

			return builder.Uri;
		}

		public virtual void SetBaseUri(string BaseUri)
		{
			this.baseUri = BaseUri;
		}

		public async Task<JObject> ExecuteRequest(string Method)
		{
			return await ExecuteRequest(Method, null);
		}

		public async Task<JObject> ExecuteRequest(string Method, NameValueCollection Params)
		{
			var requestUri = getUri(Method, Params);

			string data = null;
			var currentRepeat = 0;

			while (true)
			{
				currentRepeat++;

				try
				{
					data = await client.GetStringAsync(requestUri);
				}
				catch
				{
					if (currentRepeat > maxRepeats)
						throw;

					Thread.Sleep(currentRepeat * baseRepeatInterval);
					Trace.TraceInformation("Exception in repeat " + currentRepeat);
				}

				if (data != null)
				{
					return JObject.Parse(data);
				}
			}
		}

		public async Task<T> ExecuteRequest<T>(string Method)
		{
			return await ExecuteRequest<T>(Method, null);
		}

		public async Task<T> ExecuteRequest<T>(string Method, NameValueCollection Params)
		{
			var jsonResult = await ExecuteRequest(Method, Params);
			return jsonResult.ToObject<T>();
		}
	}
}
