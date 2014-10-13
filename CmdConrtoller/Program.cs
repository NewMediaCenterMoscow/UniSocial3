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
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdConrtoller
{
	class Program
	{
		static CloudQueue taskQueue;
		static CloudQueue resultQueue;

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
			setQueues();

			var parameters = File.ReadLines(task.InputFilename);

			var maxLen = 500;
			var toLen = 10;

			long i = 0;
			foreach (var p in parameters)
			{
				var message = createMessage(task.SocialNetwork, task.Method, p);
				taskQueue.AddMessage(message, TimeSpan.FromDays(7));

				Console.WriteLine("Send message #" + i);
				i++;

				if (i % 100 == 0)
				{
					var taskQLen = getQueueLength(taskQueue);
					var resultQLen = getQueueLength(resultQueue);

					if (taskQLen > maxLen || resultQLen > maxLen)
					{
						Console.WriteLine("Sleeping: {0}, {1}", taskQLen, resultQLen);
						while (taskQLen > toLen || resultQLen > toLen)
						{
							Thread.Sleep(2000);
							taskQLen = getQueueLength(taskQueue);
							resultQLen = getQueueLength(resultQueue);
							Console.WriteLine("In sleep: {0}, {1}", taskQLen, resultQLen);
						}
						Console.WriteLine("Sleeped!");
					}
				}
			}
		}

        protected static CloudQueueMessage createMessage(SocialNetwork network, string method, string param)
        {
            var task = new CollectTask();
            task.SocialNetwork = network;
            task.Method = method;
            task.Params = param;

            var bFrmt = new BinaryFormatter();
            var outputStream = new MemoryStream();
            bFrmt.Serialize(outputStream, task);
            var result = outputStream.ToArray();

            var message = new CloudQueueMessage(result);
            return message;
        }

		protected static FileCollectTask getTask(string filename)
		{
			var data = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<FileCollectTask>(data);
		}

		protected static void setQueues()
		{
			var storageConnStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
			var tasksQueueName = ConfigurationManager.AppSettings["tasksQueueName"];
			var resultQueueName = ConfigurationManager.AppSettings["resultQueueName"];

			// Retrieve storage account from connection string
			var storageAccount = CloudStorageAccount.Parse(storageConnStr);

			// Create the queue client
			var queueClient = storageAccount.CreateCloudQueueClient();

			// Retrieve a reference to a queue
			taskQueue = queueClient.GetQueueReference(tasksQueueName);
			taskQueue.CreateIfNotExists();

			resultQueue = queueClient.GetQueueReference(resultQueueName);
			resultQueue.CreateIfNotExists();
		}

		protected static int getQueueLength(CloudQueue queue)
		{
			queue.FetchAttributes();
			var messageCount = queue.ApproximateMessageCount;

			return messageCount ?? 0;
		}

	}
}

