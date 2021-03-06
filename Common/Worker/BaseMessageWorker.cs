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

		protected int sleepInterval = 30;
        protected int maxSleepInterval = 30;
        protected int minSleepInterval = 1;
        protected int sleepIntervalStep = 5;
		protected int messagePerOneRequest = 32;
		protected CancellationTokenSource cancelTokenSource;
		protected ParallelOptions processMessageParallerOptions;

		public BaseMessageWorker(string StorageConnectionString, string QueueName)
		{
			storageConnStr = StorageConnectionString;
			queueName = QueueName;
		}

		public virtual void Stop()
		{
			cancelTokenSource.Cancel();
		}

		public virtual void Run()
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

                Trace.TraceInformation(this.ToString() + " - messages: " + messages.Count());

				if (messages.Count() != 0)
				{
                    Trace.TraceInformation(this.ToString() + " - processing...");
                    sleepInterval = minSleepInterval;

                    processQueueMessages(messages);
				}
				else
				{
                    var sleep = sleepInterval * 1000;

                    sleepInterval += sleepIntervalStep;
                    if (sleepInterval > maxSleepInterval)
                        sleepInterval = maxSleepInterval;

                    Trace.TraceInformation(this.ToString() + " - sleeping: " + sleep);
                    Thread.Sleep(sleep);
				}
			}

			Trace.TraceInformation("Stopped");
		}

		protected virtual void processQueueMessages(IEnumerable<CloudQueueMessage> messages)
		{
			Parallel.ForEach(messages, processMessageParallerOptions, processQueueMessage);
		}

		protected virtual void processQueueMessage(CloudQueueMessage message)
		{
			//Trace.TraceInformation("Get message: " + message.Id, "Information");

			queue.DeleteMessage(message);
		}
	}
}
