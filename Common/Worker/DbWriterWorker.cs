using Common.Database;
using Common.Model;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Worker
{
	public class DbWriterWorker : BaseMessageBlobWorker
	{
		ApiHelper apiHelper;
		DbWriter dbWriter;

		long counter;

		public DbWriterWorker(string StorageQueueConnectionString, string QueueName, string ContainerName, string ConnectionString)
			: base(StorageQueueConnectionString, QueueName, ContainerName)
		{
			apiHelper = new ApiHelper();
			dbWriter = new DbWriter(ConnectionString);

			counter = 0;
		}

		protected override void processMessages(IEnumerable<CloudQueueMessage> messages)
		{
			foreach (var msg in messages)
			{
				processMessage(msg);
			}
		}

		protected override void processMessage(string message)
		{
			base.processMessage(message);

			// Get message content
			var collectTaskResult = JsonConvert.DeserializeObject<CollectTaskResult>(message);
			var resuls = apiHelper.DeserializeResult(collectTaskResult.Task, collectTaskResult.SerializedResult);

			Trace.TraceInformation("Result received: " + collectTaskResult.Task.Method);

			Interlocked.Increment(ref counter);
			dbWriter.WriteObject(collectTaskResult.Task, resuls);
			Interlocked.Decrement(ref counter);

			Trace.TraceInformation("Result saved: " + collectTaskResult.Task.Method);

		}

		public long GetCounter()
		{
			return counter;
		}
	}
}
