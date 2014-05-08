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
			var client = getQueueClient(storageConnStr);
			
			var tasksQueueName = ConfigurationManager.AppSettings["tasksQueueName"];
			var resultQueueName = ConfigurationManager.AppSettings["resultQueueName"];

			var resultQueue = client.GetQueueReference(resultQueueName);
			var taskQueue = client.GetQueueReference(tasksQueueName);

			Console.WriteLine("Tasks: {0}", taskQueue.ApproximateMessageCount);
			Console.WriteLine("Results: {0}", resultQueue.ApproximateMessageCount);

			Console.ReadLine();
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
