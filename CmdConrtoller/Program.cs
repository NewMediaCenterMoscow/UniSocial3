using Common;
using Common.Model;
using Common.Worker;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdConrtoller
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Specify task filename");
				Environment.Exit(1);
			}

			var taskFilename = args[0];
			var task = getTask(taskFilename);

			sendTask(task);
		}

		protected static void sendTask(FileCollectTask task)
		{
			var queue = getQueue();

			var parameters = File.ReadLines(task.InputFilename);

			foreach (var p in parameters)
			{
				var message = createMessage(task.SocialNetwork, task.Method, p);

				queue.AddMessage(message, TimeSpan.FromDays(7));
			}
		}

		protected static CloudQueueMessage createMessage(SocialNetwork network, string method, string param)
		{
			var task = new CollectTask();
			task.SocialNetwork = network;
			task.Method = method;
			task.Params = param;
			var messageString = JsonConvert.SerializeObject(task);

			var cbMesage = CloudQueueBlobMessage.CreateMessageWithContent(messageString);
			var cmMessageString = JsonConvert.SerializeObject(cbMesage);

			var message = new CloudQueueMessage(cmMessageString);

			return message;
		}

		protected static FileCollectTask getTask(string filename)
		{
			var data = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<FileCollectTask>(data);
		}

		protected static CloudQueue getQueue()
		{
			var storageConnStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
			var tasksQueueName = ConfigurationManager.AppSettings["tasksQueueName"];

			// Retrieve storage account from connection string
			var storageAccount = CloudStorageAccount.Parse(storageConnStr);

			// Create the queue client
			var queueClient = storageAccount.CreateCloudQueueClient();

			// Retrieve a reference to a queue
			var queue = queueClient.GetQueueReference(tasksQueueName);

			queue.CreateIfNotExists();

			return queue;
		}

	}
}

