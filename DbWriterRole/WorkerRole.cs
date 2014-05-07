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

			statusServer.Run();
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
			statusServer = new HttpStatusServer(endpoint);
			statusServer.HttpGetStatusRequest += statusServer_HttpGetStatusRequest;

			try
			{
				statusServer.Start();
			}
			catch (Exception ex)
			{
				Trace.TraceError("Status server [" + endpoint + "]: " + ex.Message);
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
			e.Writer.Write(response);
		}

	}
}
