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

namespace DbWriterRole
{
	public class WorkerRole : RoleEntryPoint
	{
		DbWriterWorker worker;

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

			worker = new DbWriterWorker(storageConnStr, resultsQueueName, containerName, dbConnectionString);
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
