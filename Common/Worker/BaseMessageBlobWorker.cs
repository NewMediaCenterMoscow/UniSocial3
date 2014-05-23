using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
    [Serializable]
	public class CloudQueueBlobMessage
	{
		public string BlobName { get; set; }
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


		protected override void processQueueMessage(CloudQueueMessage message)
		{
			base.processQueueMessage(message);

            var rawData = message.AsBytes;
            var stream = new MemoryStream(rawData);
            stream.Seek(0, SeekOrigin.Begin);
            
            var bFrmt = new BinaryFormatter();
            var resObject = bFrmt.Deserialize(stream);

            if (resObject is CloudQueueBlobMessage)
            {
                var cqbm = resObject as CloudQueueBlobMessage;
                var msgContentRef = container.GetBlockBlobReference(cqbm.BlobName);
                
                var msRes = new MemoryStream();
                msgContentRef.DownloadToStream(msRes);
                msRes.Seek(0, SeekOrigin.Begin);

                resObject = bFrmt.Deserialize(msRes);
            }

            processMessage(resObject);
		}


		protected virtual void processMessage(object rawMsgObject)
		{
			//Trace.TraceInformation("Load message: " + rawMessageContent.GetHashCode(), "Information");
		}
	}
}
