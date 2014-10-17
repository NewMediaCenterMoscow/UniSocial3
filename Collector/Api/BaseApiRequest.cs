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

		protected static int maxRepeats = 10;
		protected static int baseRepeatInterval = 300;

		public BaseApiRequest(string BaseUri)
		{
			baseUri = BaseUri;
		}

		public virtual Uri GetUri(string Method)
		{
			return GetUri(Method, new NameValueCollection());
		}

		public virtual Uri GetUri(string Method, NameValueCollection Params)
		{
			var uri = baseUri.TrimEnd('/') + '/' + Method.Trim('/');
			var builder = new UriBuilder(uri);

			builder.Query = String.Join("&", Params.AllKeys.Select(k => k + "=" + Params[k]));

			return builder.Uri;
		}

		public virtual async Task<JToken> ExecuteRequest(string Method, NameValueCollection Params)
		{
			return await excuteRequest(Method, Params);
		}

		public virtual async Task<JToken> ExecuteRequest(string Method)
		{
			return await ExecuteRequest(Method, new NameValueCollection());
		}

		public virtual async Task<T> ExecuteRequest<T>(string Method)
		{
			return await ExecuteRequest<T>(Method, new NameValueCollection());
		}

		public virtual async Task<T> ExecuteRequest<T>(string Method, NameValueCollection Params)
		{
			var jsonResult = await ExecuteRequest(Method, Params);

			return jsonResult.ToObject<T>();
		}

		protected virtual async Task<JToken> excuteRequest(string Method, NameValueCollection Params)
		{
			var requestUri = GetUri(Method, Params);

			var data = await getStringFromUri(requestUri);

			var result = JObject.Parse(data);
			return result;
		}

		protected async Task<string> getStringFromUri(Uri uri)
		{
			string data = null;
			var currentRepeat = 0;

			while (true)
			{
				currentRepeat++;

				try
				{
					data = await client.GetStringAsync(uri);
				}
				catch
				{
					if (currentRepeat > maxRepeats)
						throw;

					Thread.Sleep(baseRepeatInterval ^ currentRepeat);
					Trace.TraceInformation("Exception in repeat " + currentRepeat);
				}

				if (data != null)
				{
					return data;
				}
			}
		}
	}
}
