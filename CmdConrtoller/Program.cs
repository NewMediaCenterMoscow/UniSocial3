using Common;
using Common.Model;
using Common.Worker;
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
			//task.Method = "users.get";
			//task.Params = "1,6";
			task.Method = "groups.getMembers";
			task.Params = "68518750"; 
			var messageString = JsonConvert.SerializeObject(task);

			var cbMesage = CloudQueueBlobMessage.CreateMessageWithContent(messageString);
			var cmMessageString = JsonConvert.SerializeObject(cbMesage);

			CloudQueueMessage message = new CloudQueueMessage(cmMessageString);

			queue.AddMessage(message);

		}

	}
}

