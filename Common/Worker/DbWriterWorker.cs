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
	public class DbWriterWorker : BaseMessageBlobWorker
	{
		public DbWriterWorker(string StorageQueueConnectionString, string QueueName, string ContainerName)
			: base(StorageQueueConnectionString, QueueName, ContainerName)
		{
 
		}

		protected override void processMessage(string message)
		{
			base.processMessage(message);

			// Get message content
			var collectTaskResult = JsonConvert.DeserializeObject<CollectTaskResult>(message);


			Trace.TraceInformation("Result received: " + collectTaskResult.Task.Method);
		}
	}
}
