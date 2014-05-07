using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbWriterRole
{
	public class HttpStatusServerClient : IDisposable
	{
		Stream stream;

		TcpClient client;

		public event EventHandler<HttpGetStatusRequestEventArgs> HttpGetRequest;

		public HttpStatusServerClient(TcpClient Client)
		{
			client = Client;
			
			stream = client.GetStream();
		}

		public void HandleRequest()
		{
			var reader = new StreamReader(stream);

			var request = reader.ReadLine();
			var headers = new List<string>(10);

			var currentHeader = "";
			while (true)
			{
				currentHeader = reader.ReadLine();
				if (currentHeader != "")
					headers.Add(currentHeader);
				else
					break;
			}

			handleRequest(request, headers);

			stream.Close();
		}

		public void Dispose()
		{
			client.Close();
		}
		protected void handleRequest(string request, List<string> headers)
		{
			var outputStream = new MemoryStream();
			var outputStreamWriter = new StreamWriter(outputStream);

			if (HttpGetRequest != null)
				HttpGetRequest(this, new HttpGetStatusRequestEventArgs(outputStreamWriter));

			outputStreamWriter.Close();
			var responseContent = outputStream.ToArray();
			var responseHeaders = getResponseHeaders(responseContent.Length);

			writeResponse(responseContent, responseHeaders);
		}

		protected void writeResponse(byte[] responseContent, List<string> responseHeaders)
		{
			var streamWriter = new StreamWriter(stream);

			streamWriter.Write("HTTP/1.1 200 OK\r\n");
			foreach (var item in responseHeaders)
			{
				streamWriter.Write(item + "\r\n");
			}
			streamWriter.Write("\r\n");
			streamWriter.Write(Encoding.UTF8.GetString(responseContent));
			streamWriter.Flush();
			streamWriter.Close();
		}

		protected List<string> getResponseHeaders(int contentLenght)
		{
			return new List<string>() { 
				"Server: NMC-HttpStatusServer",
				"Content-Type: application/json; charset=utf-8",
				"Connection: close",
				"Content-Length: " + contentLenght
			};
		}

	}

	public class HttpGetStatusRequestEventArgs
	{
		public StreamWriter Writer;

		public HttpGetStatusRequestEventArgs(StreamWriter OutputStreamWriter)
		{
			this.Writer = OutputStreamWriter;
		}
	}

	public class HttpStatusServer
	{
		TcpListener listener;
		bool isListening;

		public event EventHandler<HttpGetStatusRequestEventArgs> HttpGetStatusRequest;

		public HttpStatusServer(IPEndPoint Endpoint)
		{
			isListening = false;

			listener = new TcpListener(IPAddress.Any, Endpoint.Port);
		}

		public void Start()
		{
			isListening = true;
			listener.Start();

			Trace.TraceInformation("HttpStatusServer run on : " + listener.LocalEndpoint);
		}

		public void Stop()
		{
			isListening = false;
		}

		public void Run()
		{
			Task.Run(async () =>
			{
				while (isListening)
				{
					#region HttpListened
					//var ctx = await listener.GetContextAsync();

					//if (HttpGetStatusRequest != null)
					//	HttpGetStatusRequest(this, new HttpGetStatusRequestEventArgs(ctx.Response.OutputStream));

					//ctx.Response.OutputStream.Close();
					//Thread.Sleep(10);
					#endregion

					var tcpClient = await listener.AcceptTcpClientAsync();
					var client = new HttpStatusServerClient(tcpClient);
					client.HttpGetRequest += (s, e) =>
					{
						if (HttpGetStatusRequest != null) HttpGetStatusRequest(s, e);
					};
					Task.Run((Action)client.HandleRequest);

					Thread.Sleep(10);
				}

				// isListening = false => HttpStatusServer.Stop was called
				listener.Stop();
			});
		}

	}
}
