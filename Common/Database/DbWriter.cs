using Collector.Models.Vk;
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
using System.Threading.Tasks;

namespace Common.Database
{
	public class DbWriter : IDisposable
	{
		protected string connStr;
		protected SqlConnection sqlConn;

		protected Dictionary<string, Tuple<string, List<Tuple<string, SqlDbType>>>> commands;
		protected Dictionary<string, DataTable> dataTables;


		protected string settingsFilename = "Database/settings.json";

		public DbWriter(string ConnectionString)
		{
			connStr = ConnectionString;

			setConnection();

			commands = new Dictionary<string, Tuple<string, List<Tuple<string, SqlDbType>>>>();
			dataTables = new Dictionary<string, DataTable>();
			loadSettings();
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

		protected void setConnection()
		{
			if (sqlConn == null)
				sqlConn = new SqlConnection(connStr);

		}

		protected void checkConnection()
		{
			if (sqlConn.State == ConnectionState.Closed)
				sqlConn.Open();
		}

		protected void loadSettings()
		{
			var json = File.ReadAllText(settingsFilename);
			var jsonSettings = JObject.Parse(json);

			var param = from item in jsonSettings["items"]
						select new
						{
							network = (SocialNetwork)Enum.Parse(typeof(SocialNetwork), (string)item["network"]),
							method = (string)item["method"],
							table = (string)item["table"],
							fields = from prop in item["fields"] 
									 let jprop = prop as JProperty
									 select new Tuple<string, SqlDbType>(jprop.Name, (SqlDbType)Enum.Parse(typeof(SqlDbType), (string)jprop.Value))
						};

			foreach (var p in param)
			{
				addParam(p.network, p.method, p.table, p.fields.ToList());
			}
		}

		private void addParam(SocialNetwork socialNetwork, string method, string table, List<Tuple<string, SqlDbType>> fields)
		{
			var key = ApiHelper.GetKey(socialNetwork, method);

			var queryStart = "INSERT INTO " + table;
			var fs = String.Join(",", fields.Select(ft => ft.Item1));
			var fp = String.Join(",", fields.Select(ft => "@" + ft.Item1));
			var query = queryStart + " (" + fs + ") VALUES (" + fp + ")";

			commands.Add(key, new Tuple<string, List<Tuple<string, SqlDbType>>>(query, fields));

			var dataTable = new DataTable(table);
			foreach (var field in fields)
				dataTable.Columns.Add(field.Item1);

			dataTables.Add(key, dataTable);
		}

		protected Tuple<SqlCommand, SqlBulkCopy, DataTable> createCommand(CollectTask task)
		{
			var key = ApiHelper.GetKey(task.SocialNetwork, task.Method);

			var query = commands[key].Item1;
			var sqlParamsTuple = commands[key].Item2;

			var sqlParams = sqlParamsTuple.Select(pt => new SqlParameter("@" + pt.Item1, pt.Item2));//from pt in sqlParamsTuple select new SqlParameter("@" + pt.Item1, pt.Item2);

			var sqlCmd = new SqlCommand(query, sqlConn);
			sqlCmd.Parameters.AddRange(sqlParams.ToArray());


			var sqlBulk = new SqlBulkCopy(sqlConn);
			sqlBulk.DestinationTableName = dataTables[key].TableName;
			var bulkMappings = sqlParamsTuple.Select((pt, index) => new SqlBulkCopyColumnMapping(index, pt.Item1));

			foreach (var item in bulkMappings)
				sqlBulk.ColumnMappings.Add(item);

			return new Tuple<SqlCommand, SqlBulkCopy, DataTable>(sqlCmd, sqlBulk, dataTables[key]);
		}

		public void WriteObject(CollectTask task, object data)
		{
			checkConnection();

			var sqlCmds = createCommand(task);

			if (data is VkList<long>)
			{
				writeObjects(task, data as VkList<long>, sqlCmds);
			}
		}

		protected void writeObjects(CollectTask task, VkList<long> data, Tuple<SqlCommand, SqlBulkCopy, DataTable> sqlCmds)
		{
			var batchSize = 300;
			var groupedItems = data.Items
				.Select((item, index) => new { Index = index, Item = item })
				.GroupBy(di => di.Index / batchSize)
				;

			var insertCmd = sqlCmds.Item1;
			var bulkCopy = sqlCmds.Item2;
			var dataTable = sqlCmds.Item3;

			var p1 = long.Parse(task.Params);

			// First, try to insert group in batch mode
			foreach (var item in groupedItems)
			{
				dataTable.Clear();
				foreach (var row in item)
				{
					dataTable.Rows.Add(p1, row.Item);
				}

				try
				{
					bulkCopy.WriteToServer(dataTable);
				}
				catch (SqlException) // Exception - insert row-by-row
				{
					Trace.TraceWarning("Bulk exception: insert rows");

					foreach (var row in item)
					{
						insertCmd.Parameters[0].Value = p1;
						insertCmd.Parameters[1].Value = row.Item;

						try
						{
							insertCmd.ExecuteNonQuery();
						}
						catch (SqlException ex)
						{
							Trace.TraceError("Insert exception: " + ex.Message);
						}
					}

				}
			}


		}



	}
}
