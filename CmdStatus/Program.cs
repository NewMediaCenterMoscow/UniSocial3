using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdStatus
{
	class Program
	{
		static void Main(string[] args)
		{
			var storageConnStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
			var resultQueueName = ConfigurationManager.AppSettings["resultQueueName"];
			var tasksQueueName = ConfigurationManager.AppSettings["tasksQueueName"];
			
			var client = getQueueClient(storageConnStr);
			var taskQueue = client.GetQueueReference(tasksQueueName);
			var resultQueue = client.GetQueueReference(resultQueueName);

			var taskMessages = getQueueLength(taskQueue);
			var resultMessages = getQueueLength(resultQueue);

			Console.WriteLine("Tasks: {0}", taskMessages);
			Console.WriteLine("Results: {0}", resultMessages);
		}

		protected static int getQueueLength(CloudQueue queue)
		{
			queue.FetchAttributes();
			var messageCount = queue.ApproximateMessageCount;

			return messageCount ?? 0;
		}

		protected static CloudQueueClient getQueueClient(string storageConnStr)
		{
			// Retrieve storage account from connection string
			var storageAccount = CloudStorageAccount.Parse(storageConnStr);

			// Create the queue client
			var queueClient = storageAccount.CreateCloudQueueClient();

			return queueClient;
		}
	}
}
