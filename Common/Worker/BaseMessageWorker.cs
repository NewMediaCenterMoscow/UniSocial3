﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Worker
{
	public class BaseMessageWorker
	{
		protected CloudStorageAccount storageAccount;
		protected string storageConnStr;
		protected string queueName;
		protected CloudQueueClient queueClient;
		protected CloudQueue queue;

		protected int baseSleepInterval = 5000;
		protected int messagePerOneRequest = 32;
		protected CancellationTokenSource cancelTokenSource;
		protected ParallelOptions processMessageParallerOptions;

		public BaseMessageWorker(string StorageConnectionString, string QueueName)
		{
			storageConnStr = StorageConnectionString;
			queueName = QueueName;
		}

		public void Stop()
		{
			cancelTokenSource.Cancel();
		}

		public void Run()
		{
			processMessageParallerOptions = new ParallelOptions();
			processMessageParallerOptions.MaxDegreeOfParallelism = 5;

			cancelTokenSource = new CancellationTokenSource();
			var token = cancelTokenSource.Token;
			var task = Task.Run(() => workCycle(token), token);
			task.Wait();
		}

		public virtual void Initialize()
		{
			// Retrieve storage account from connection string
			this.storageAccount = CloudStorageAccount.Parse(this.storageConnStr);

			configureQueue(this.storageAccount);
		}

		protected virtual void configureQueue(CloudStorageAccount storageAccount)
		{
			// Create the queue client
			this.queueClient = storageAccount.CreateCloudQueueClient();

			this.queue = createQueue(this.queueClient, this.queueName);
		}
		
		protected virtual CloudQueue createQueue(CloudQueueClient client, string queueName)
		{
			// Retrieve a reference to a queues
			var queue = client.GetQueueReference(queueName);

			// Create the queue if it doesn't already exist
			queue.CreateIfNotExists();

			return queue;
		}

		protected virtual void workCycle(CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				var messages = queue.GetMessages(messagePerOneRequest);
				Parallel.ForEach(messages, processMessageParallerOptions, processMessage);

				Thread.Sleep(baseSleepInterval);
			}

			Trace.TraceInformation("Stopped");
		}

		protected virtual void processMessage(CloudQueueMessage message)
		{
			Trace.TraceInformation("Get message: " + message.Id, "Information");

			queue.DeleteMessage(message);
		}
	}
}
