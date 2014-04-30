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
		ApiHelper apiHelper;

		public DbWriterWorker(string StorageQueueConnectionString, string QueueName, string ContainerName)
			: base(StorageQueueConnectionString, QueueName, ContainerName)
		{
			apiHelper = new ApiHelper();
		}

		protected override void processMessage(string message)
		{
			base.processMessage(message);

			// Get message content
			var collectTaskResult = JsonConvert.DeserializeObject<CollectTaskResult>(message);

			var resuls = apiHelper.DeserializeResult(collectTaskResult.Task, collectTaskResult.SerializedResult);

			Trace.TraceInformation("Result received: " + collectTaskResult.Task.Method);
		}
	}
}
