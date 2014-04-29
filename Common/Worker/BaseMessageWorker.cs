using Microsoft.WindowsAzure.Storage;
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
		protected string storageConnStr;
		protected string queueName;
		protected CloudQueueClient queueClient;
		protected CloudQueue queue;

		protected int baseSleepInterval = 5000;
		protected int messagePerOneRequest = 32;
		protected CancellationTokenSource cancelTokenSource;
		protected ParallelOptions processMessageParallerOptions;

		public BaseMessageWorker(string StorageQueueConnectionString, string QueueName)
		{
			storageConnStr = StorageQueueConnectionString;
			queueName = QueueName;
		}

		public void Initialize()
		{
			configureQueue();
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


		protected virtual CloudQueueClient createQueueClient(string storageConnStr)
		{
			// Retrieve storage account from connection string
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnStr);

			// Create the queue client
			var client = storageAccount.CreateCloudQueueClient();

			return client;
		}

		protected virtual CloudQueue createQueue(CloudQueueClient client, string queueName)
		{
			// Retrieve a reference to a queues
			var queue = client.GetQueueReference(queueName);

			// Create the queue if it doesn't already exist
			queue.CreateIfNotExists();

			return queue;
		}

		protected virtual void configureQueue()
		{
			this.queueClient = createQueueClient(this.storageConnStr);
			this.queue = createQueue(this.queueClient, this.queueName);
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
