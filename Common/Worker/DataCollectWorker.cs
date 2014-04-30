using Common.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Worker
{
	public class DataCollectWorker : BaseMessageBlobWorker
	{
		protected string resultQueueName;
		protected CloudQueue resultQueue;

		ApiHelper apiHelper;

		public DataCollectWorker(string StorageQueueConnectionString, string TaskQueueName, string ResultQueueName, string ContainerName)
			: base(StorageQueueConnectionString, TaskQueueName, ContainerName)
		{
			resultQueueName = ResultQueueName;

			apiHelper = new ApiHelper();
		}


		protected override void configureQueue(CloudStorageAccount storageAccount)
		{
			// Create base queue
			base.configureQueue(storageAccount);

			// Create additional queue to write results
			this.resultQueue = createQueue(this.queueClient, this.resultQueueName);
		}

		protected override void processMessage(string message)
		{
			base.processMessage(message);

			var collectTask = JsonConvert.DeserializeObject<CollectTask>(message);

			collectData(collectTask);
		}


		protected void collectData(CollectTask collectTask)
		{
			var result = apiHelper.GetResult(collectTask);
			var strResult = JsonConvert.SerializeObject(result);

			var messageToSend = new CollectTaskResult();
			messageToSend.Task = collectTask;
			messageToSend.SerializedResult = strResult;

			sendResult(messageToSend);
		}

		private void sendResult(CollectTaskResult result)
		{
			var msgStr = JsonConvert.SerializeObject(result);
			CloudQueueMessage msg = formatMessageWitResult(msgStr);

			resultQueue.AddMessage(msg);

			Trace.TraceInformation("Result sended: " + result.Task.Method);
		}

		CloudQueueMessage formatMessageWitResult(string data)
		{
			CloudQueueBlobMessage qbMessage = null;
			if (data.Length < 30000) // Message is small - send it directly
			{
				qbMessage = CloudQueueBlobMessage.CreateMessageWithContent(data);
			}
			else // Message is too large - send it with blob
			{
				var uniqBytes = UTF8Encoding.UTF8.GetBytes(data.GetHashCode().ToString() + DateTime.Now.ToString());
				var blobName = Convert.ToBase64String(uniqBytes);
				var blob = container.GetBlockBlobReference(blobName);
				blob.UploadText(data);

				qbMessage = CloudQueueBlobMessage.CreateMessageWithBlob(blobName);
			}

			var cloudMessage = new CloudQueueMessage(JsonConvert.SerializeObject(qbMessage));
			return cloudMessage;
		}

	}
}
