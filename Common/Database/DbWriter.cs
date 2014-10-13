using Collector.Models.Vk;
using Common.Database.Writers;
using Common.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Database
{
	public class DbWriter : IDisposable
	{
		protected SqlConnection sqlConn;
		protected ObjectWritersFactory writerFactory;

		public DbWriter(string ConnectionString)
		{
			sqlConn = new SqlConnection(ConnectionString);
			writerFactory = new ObjectWritersFactory(sqlConn);
		}

		public void Dispose()
		{
			if (sqlConn != null)
			{
				if (sqlConn.State != ConnectionState.Closed)
					sqlConn.Close();

				sqlConn.Dispose();
			}
		}

		protected void checkConnection()
		{
			if (sqlConn.State != ConnectionState.Open)
			{
				sqlConn.Open();
			}
		}

		public void WriteObject(CollectTask task, object data)
		{
			checkConnection();

			IObjectWriter writer = writerFactory.CreateWriter(task);
			writer.WriteData(task, data);
		}

	
	}
}
