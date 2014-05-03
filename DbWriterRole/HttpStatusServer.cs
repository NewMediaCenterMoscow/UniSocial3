using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbWriterRole
{
	public class HttpGetStatusRequestEventArgs
	{
		public Stream OutputStream;

		public HttpGetStatusRequestEventArgs(Stream OutputStream)
		{
			this.OutputStream = OutputStream;
		}
	}

	public class HttpStatusServer
	{
		HttpListener listener;
		bool isListening;

		public event EventHandler<HttpGetStatusRequestEventArgs> HttpGetStatusRequest;

		public HttpStatusServer(string UrlPrefix)
		{
			isListening = false;
			listener = new HttpListener();

			listener.Prefixes.Add(UrlPrefix);
		}

		public void Start()
		{
			isListening = true;
			listener.Start();

			Task.Run(async () =>
			{
				while (isListening)
				{
					var ctx = await listener.GetContextAsync();

					if (HttpGetStatusRequest != null)
						HttpGetStatusRequest(this, new HttpGetStatusRequestEventArgs(ctx.Response.OutputStream));

					ctx.Response.OutputStream.Close();
					Thread.Sleep(10);
				}

				int i = 0;
			});
		}

		public void Stop()
		{
			isListening = false;
			listener.Stop();
		}

	}
}
