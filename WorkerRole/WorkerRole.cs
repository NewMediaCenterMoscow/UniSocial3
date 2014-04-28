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
using Common.ApiHelper;

namespace WorkerRole
{
	public class WorkerRole : RoleEntryPoint
	{
		bool isStopped;

		string storageConnStr;
		string tasksQueueName;
		string resultsQueueName;
		CloudQueueClient queueClient;
		CloudQueue taskQueue;
		CloudQueue resultQueue;

		int sleepInterval;

		int messagePerOneRequest = 32;

		ApiHelper apiHelper;

		public override void Run()
		{
			// This is a sample worker implementation. Replace with your logic.
			Trace.TraceInformation("WorkerRole entry point called", "Information");

			RunAsync(); // Sync, in fact
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			sleepInterval = 10000;
			isStopped = false;

			createQueue();

			apiHelper = new ApiHelper();

			return base.OnStart();
		}

		public override void OnStop()
		{
			isStopped = true;

			base.OnStop();
		}

		public void RunAsync()
		{
			while (!isStopped)
			{
				// Get messages
				var messages = taskQueue.GetMessages(messagePerOneRequest);
				processMessages(messages);

				Trace.TraceInformation("Working", "Information");
				if (sleepInterval != 0)
				{
					Thread.Sleep(sleepInterval);
				}
			}
		}


		private void createQueue()
		{
			storageConnStr = 
				RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
			tasksQueueName =
				RoleEnvironment.GetConfigurationSettingValue("tasksQueueName");
			resultsQueueName =
				RoleEnvironment.GetConfigurationSettingValue("resultsQueueName");

			// Retrieve storage account from connection string
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnStr);

			// Create the queue client
			CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

			// Retrieve a reference to a queues
			taskQueue = queueClient.GetQueueReference(tasksQueueName);
			resultQueue = queueClient.GetQueueReference(resultsQueueName);

			// Create the queue if it doesn't already exist
			taskQueue.CreateIfNotExists();
			resultQueue.CreateIfNotExists();
		}


		private void processMessages(IEnumerable<CloudQueueMessage> messages)
		{
			foreach (var msg in messages)
			{
				processMessage(msg);
			}
		}

		private void processMessage(CloudQueueMessage message)
		{
			// Get message content
			var content = message.AsString;
			var collectTask = JsonConvert.DeserializeObject<CollectTask>(content);

			Trace.TraceInformation("Get message: " + collectTask.ToString(), "Information");

			var result = apiHelper.GetResult(collectTask);

			var str = result.ToString();

			// Delete message
			taskQueue.DeleteMessage(message);
		}


	}
}
