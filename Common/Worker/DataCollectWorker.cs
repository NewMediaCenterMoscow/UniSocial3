using Common.Model;
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
	public class DataCollectWorker : BaseMessageWorker
	{
		protected string resultQueueName;
		protected CloudQueue resultQueue;

		ApiHelper apiHelper;

		public DataCollectWorker(string StorageQueueConnectionString, string TaskQueueName, string ResultQueueName)
			: base(StorageQueueConnectionString, TaskQueueName)
		{
			resultQueueName = ResultQueueName;

			apiHelper = new ApiHelper();
		}


		protected override void configureQueue()
		{
			// Create base queue
			base.configureQueue();

			// Create additional queue to write results
			this.resultQueue = createQueue(this.queueClient, this.resultQueueName);
		}

		protected override void processMessage(CloudQueueMessage message)
		{
			base.processMessage(message);

			// Get message content
			var content = message.AsString;
			var collectTask = JsonConvert.DeserializeObject<CollectTask>(content);

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
			CloudQueueMessage msg = new CloudQueueMessage(msgStr);

			resultQueue.AddMessage(msg);

			Trace.TraceInformation("Result sended: " + result.Task.Method);
		}
	}
}
