using Common.Helpers;
using Common.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        protected override void processQueueMessages(IEnumerable<CloudQueueMessage> messages)
        {
            foreach (var msg in messages)
            {
                processQueueMessage(msg);
            }
        }

        protected override void processMessage(object message)
		{
			base.processMessage(message);

            var collectTask = message as CollectTask;

			Trace.TraceInformation("Task: " + collectTask.Method);

			collectData(collectTask);
		}


		protected void collectData(CollectTask collectTask)
		{
			var result = apiHelper.GetResult(collectTask);

			if (result != null)
			{
				var messageToSend = new CollectTaskResult();
                messageToSend.Result = result;
				messageToSend.Task = collectTask;

				sendResult(messageToSend);
			}
		}

		private void sendResult(CollectTaskResult result)
		{
            var bFrmt = new BinaryFormatter();
            var outputStream = new MemoryStream();
            bFrmt.Serialize(outputStream, result);
            var byteResult = outputStream.ToArray();

            CloudQueueMessage msg = formatMessageWitResult(byteResult);
			resultQueue.AddMessage(msg);

			Trace.TraceInformation("Result sended: " + result.Task.Method);
		}

		CloudQueueMessage formatMessageWitResult(byte[] data)
		{
            CloudQueueMessage qMessage = null;

            if (CloudQueueHelper.IsAllowedQueueMessageSize(data.LongLength))
            {
                qMessage = new CloudQueueMessage(data);
            }
            else
            {
                var uniqBytes = UTF8Encoding.UTF8.GetBytes(data.GetHashCode().ToString() + DateTime.Now.ToString());
                var blobName = Convert.ToBase64String(uniqBytes);
                var blob = container.GetBlockBlobReference(blobName);
                blob.UploadFromByteArray(data, 0, data.Length);

                var cqbm = new CloudQueueBlobMessage();
                cqbm.BlobName = blobName;

                var bFrmt = new BinaryFormatter();
                var outputStream = new MemoryStream();
                bFrmt.Serialize(outputStream, cqbm);
                var byteResult = outputStream.ToArray();

                qMessage = new CloudQueueMessage(byteResult);
            }

            return qMessage;
		}

	}
}
