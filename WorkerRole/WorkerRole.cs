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
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Common;
using System.Threading.Tasks;
using Common.Model;
using Common.Worker;

namespace WorkerRole
{
	public class WorkerRole : RoleEntryPoint
	{
		DataCollectWorker worker;

		public override void Run()
		{
			// This is a sample worker implementation. Replace with your logic.
			Trace.TraceInformation("WorkerRole entry point called", "Information");

			worker.Run();
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			var storageConnStr = 
				RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
			var tasksQueueName =
				RoleEnvironment.GetConfigurationSettingValue("tasksQueueName");
			var resultsQueueName =
				RoleEnvironment.GetConfigurationSettingValue("resultsQueueName");
			var containerName =
				RoleEnvironment.GetConfigurationSettingValue("resultQueueContainerName");

			worker = new DataCollectWorker(storageConnStr, tasksQueueName, resultsQueueName, containerName);
			worker.Initialize();

			return base.OnStart();
		}

		public override void OnStop()
		{
			worker.Stop();

			base.OnStop();
		}
	}
}
