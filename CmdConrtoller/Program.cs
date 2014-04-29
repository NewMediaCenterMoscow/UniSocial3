using Common;
using Common.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdConrtoller
{
	class Program
	{
		static string storageConnStr;
		static string tasksQueueName;
		static string resultsQueueName;


		static void Main(string[] args)
		{
			sendMessageToQueue();

			//testApiHelper();
		}

		private static void testApiHelper()
		{
			var apiHelper = new ApiHelper();

			var task = new CollectTask();
			task.SocialNetwork = SocialNetwork.VKontakte;
			task.Method = "groups.getMembers";
			task.Params = "1";

			try
			{
				var result = apiHelper.GetResult(task);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		static void configureQueue()
		{
			storageConnStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
			tasksQueueName = ConfigurationManager.AppSettings["tasksQueueName"];
			resultsQueueName = ConfigurationManager.AppSettings["resultsQueueName"];
		}

		private static void sendMessageToQueue()
		{
			configureQueue();

			// Retrieve storage account from connection string
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnStr);

			// Create the queue client
			CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

			// Retrieve a reference to a queue
			CloudQueue queue = queueClient.GetQueueReference(tasksQueueName);

			// Create the queue if it doesn't already exist
			queue.CreateIfNotExists();

			// Send simple task

			var task = new CollectTask();
			task.SocialNetwork = SocialNetwork.VKontakte;
			task.Method = "groups.getMembers";
			task.Params = "1";

			var messageString = JsonConvert.SerializeObject(task);

			CloudQueueMessage message = new CloudQueueMessage(messageString);



			queue.AddMessage(message);

		}

	}
}

