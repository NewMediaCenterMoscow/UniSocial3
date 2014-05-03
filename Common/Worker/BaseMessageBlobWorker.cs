using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
	public class CloudQueueBlobMessage
	{
		private CloudQueueBlobMessage()
		{

		}

		public static CloudQueueBlobMessage CreateMessageWithContent(string Content)
		{
			var message = new CloudQueueBlobMessage();
			message.Content = Content;
			message.BlobName = null;

			return message;
		}

		public static CloudQueueBlobMessage CreateMessageWithBlob(string BlobName)
		{
			var message = new CloudQueueBlobMessage();
			message.Content = null;
			message.BlobName = BlobName;

			return message;
		}

		public string Content { get; set; }
		public string BlobName { get; set; }

		public bool IsInBlob
		{
			get
			{
				return BlobName != null;
			}
		}
	}

	public class BaseMessageBlobWorker : BaseMessageWorker
	{
		protected string containerName;
		protected CloudBlobClient blobClient;
		protected CloudBlobContainer container;

		public BaseMessageBlobWorker(string StorageConnectionString, string QueueName, string ContainerName)
			: base(StorageConnectionString, QueueName)
		{
			containerName = ContainerName;
		}

		public override void Initialize()
		{
			base.Initialize();

			configureBlob(this.storageAccount);
		}

		protected virtual void configureBlob(CloudStorageAccount storageAccount)
		{
			this.blobClient = this.storageAccount.CreateCloudBlobClient();

			this.container = createContainer(this.blobClient, this.containerName);
		}

		protected virtual CloudBlobContainer createContainer(CloudBlobClient client, string containerName)
		{
			// Retrieve a reference to a container. 
			var container = blobClient.GetContainerReference(containerName);

			// Create the container if it doesn't already exist.
			container.CreateIfNotExists();

			return container;
		}


		protected override void processMessage(CloudQueueMessage message)
		{
			base.processMessage(message);

			// Get message content
			var content = message.AsString;
			var cbMessage = JsonConvert.DeserializeObject<CloudQueueBlobMessage>(content);

			string msgContent = null;

			if (!cbMessage.IsInBlob)
			{
				msgContent = cbMessage.Content;
			}
			else
			{
				var msgContentRef = container.GetBlockBlobReference(cbMessage.BlobName);
				msgContent = msgContentRef.DownloadText();

				msgContentRef.Delete();
			}

			processMessage(msgContent);
		}


		protected virtual void processMessage(string rawMessageContent)
		{
			//Trace.TraceInformation("Load message: " + rawMessageContent.GetHashCode(), "Information");
		}
	}
}
