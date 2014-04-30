using Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Worker
{
	public class DbWriterWorker : BaseMessageWorker
	{
		public DbWriterWorker(string StorageQueueConnectionString, string QueueName)
			: base(StorageQueueConnectionString, QueueName)
		{
 
		}

		protected override void processMessage(Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage message)
		{
			base.processMessage(message);

			// Get message content
			var content = message.AsString;
			var collectTaskResult = JsonConvert.DeserializeObject<CollectTaskResult>(content);

			Trace.TraceInformation("Result received: " + collectTaskResult.Task.Method);
		}
	}
}
