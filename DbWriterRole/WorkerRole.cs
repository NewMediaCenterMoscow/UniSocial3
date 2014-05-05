using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Common.Worker;
using System.Threading.Tasks;
using System.Text;

namespace DbWriterRole
{
	public class WorkerRole : RoleEntryPoint
	{
		DbWriterWorker worker;
		HttpStatusServer statusServer;

		public override void Run()
		{
			// This is a sample worker implementation. Replace with your logic.
			Trace.TraceInformation("DbWriterRole entry point called", "Information");

			worker.Run();
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			var storageConnStr =
				RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
			var resultsQueueName =
				RoleEnvironment.GetConfigurationSettingValue("resultsQueueName");
			var containerName =
				RoleEnvironment.GetConfigurationSettingValue("resultQueueContainerName");
			var dbConnectionString =
				RoleEnvironment.GetConfigurationSettingValue("DbConnectionString");
			var statusEndpoint =
				RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["StatusEndpoint"];

			worker = new DbWriterWorker(storageConnStr, resultsQueueName, containerName, dbConnectionString);
			worker.Initialize();

			var endpoint = statusEndpoint.PublicIPEndpoint ?? statusEndpoint.IPEndpoint;
			statusServer = new HttpStatusServer(new string[] { "http://*:" + endpoint.Port + "/", "http://unisocial.cloudapp.net" + endpoint.Port + "/" });
			statusServer.HttpGetStatusRequest += statusServer_HttpGetStatusRequest;

			try
			{
				statusServer.Start();
			}
			catch (HttpListenerException ex)
			{
				Trace.TraceWarning("HttpListener: " + ex.Message);
			}

			return base.OnStart();
		}

		public override void OnStop()
		{
			worker.Stop();
			statusServer.Stop();

			base.OnStop();
		}

		void statusServer_HttpGetStatusRequest(object sender, HttpGetStatusRequestEventArgs e)
		{
			string response = "{\"counter\":" + worker.GetCounter() + "}";
			var respBytes = Encoding.UTF8.GetBytes(response);

			e.OutputStream.Write(respBytes, 0, respBytes.Length);
		}

	}
}
