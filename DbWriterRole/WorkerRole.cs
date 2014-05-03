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
		HttpListener listener;

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
			var statusENdpoint =
				RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["StatusEndpoint"].IPEndpoint;

			worker = new DbWriterWorker(storageConnStr, resultsQueueName, containerName, dbConnectionString);
			worker.Initialize();

			createHttpStatusServer(statusENdpoint);

			return base.OnStart();
		}

		public override void OnStop()
		{
			worker.Stop();
			listener.Stop();


			base.OnStop();
		}

		private void createHttpStatusServer(IPEndPoint statusENdpoint)
		{
			HttpListener listener = new HttpListener();

			listener.Start();

			listener.Prefixes.Add("http://*:" + statusENdpoint.Port + "/");

			Task.Run(async () => {
				while (true) {
					var ctx = await listener.GetContextAsync();

					string response = "{counter:" + worker.GetCounter() + "}";
					var respBytes = Encoding.UTF8.GetBytes(response);

					ctx.Response.OutputStream.Write(respBytes, 0, respBytes.Length);
					ctx.Response.OutputStream.Close();

					Thread.Sleep(10);
				}
			});
		}

	}
}
