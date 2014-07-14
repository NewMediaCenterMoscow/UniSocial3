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
using System.Threading;
using System.Threading.Tasks;

namespace Common.Database
{
	public class SqlFieldDescription
	{
		public string Name { get; set; }
		public SqlDbType Type { get; set; }
	}
	public class SqlTableDescription
	{
		public string TableName { get; set; }
		public string InsertStatement { get; set; }
		public List<SqlFieldDescription> Fields { get; set; }
	}
	public class DbWriteCommand
	{
		public SqlCommand InsertComamnd { get; set; }
		public SqlBulkCopy BulkCopy { get; set; }
		public DataTable Table { get; set; }
	}

	public class DbWriter : IDisposable
	{
		protected SqlConnection sqlConn;

		protected Dictionary<string, SqlTableDescription> commands;
		protected delegate void addDataRowDelegate(DataTable dt, object data);
		protected delegate void setSqlParamsDelegate(SqlCommand cmd, object data);

		protected string settingsFilename = "Database/settings.json";

		public DbWriter(string ConnectionString)
		{
			setConnection(ConnectionString);
			loadSettings(settingsFilename);
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

		protected void setConnection(string connStr)
		{
			if (sqlConn == null)
				sqlConn = new SqlConnection(connStr);
		}

		protected void checkConnection()
		{
			if (sqlConn.State != ConnectionState.Open)
			{
				sqlConn.Open();
			}
		}

		protected void loadSettings(string filename)
		{
			var json = File.ReadAllText(filename);
			var jsonSettings = JObject.Parse(json);

			var param = from item in jsonSettings["items"]
						select new
						{
							network = (SocialNetwork)Enum.Parse(typeof(SocialNetwork), (string)item["network"], true),
							method = (string)item["method"],
							table = (string)item["table"],
							fields = from prop in item["fields"] 
									 let jprop = prop as JProperty
									 select new SqlFieldDescription()
									 {
										 Name = jprop.Name,
										 Type = (SqlDbType)Enum.Parse(typeof(SqlDbType), (string)jprop.Value, true)
									 }
						};

			commands = new Dictionary<string, SqlTableDescription>();
			foreach (var p in param)
			{
				var key = ApiHelper.GetKey(p.network, p.method);
				var tableDesc = createSqlTableDescription(p.network, p.method, p.table, p.fields.ToList());
				commands.Add(key, tableDesc);
			}
		}

		private SqlTableDescription createSqlTableDescription(SocialNetwork socialNetwork, string method, string table, List<SqlFieldDescription> fields)
		{
			var queryStart = "INSERT INTO " + table;
			var fs = String.Join(",", fields.Select(ft => ft.Name));
			var fp = String.Join(",", fields.Select(ft => "@" + ft.Name));
			var query = queryStart + " (" + fs + ") VALUES (" + fp + ")";

			var result = new SqlTableDescription()
			{
				TableName = table,
				InsertStatement = query,
				Fields = fields
			};

			return result;
		}

		protected DbWriteCommand createCommand(CollectTask task)
		{
			var key = ApiHelper.GetKey(task.SocialNetwork, task.Method);

			var query = commands[key].InsertStatement;
			var fields = commands[key].Fields;
			var tableName = commands[key].TableName;

			var sqlCmd = new SqlCommand(query, sqlConn);
			var dataTable = new DataTable(tableName);
			var sqlBulk = new SqlBulkCopy(sqlConn);
			sqlBulk.DestinationTableName = tableName;

			for(int i=0;i<fields.Count;i++)
			{
				var field = fields[i];

				sqlCmd.Parameters.Add(new SqlParameter("@" + field.Name, field.Type));
				sqlBulk.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, field.Name));
				dataTable.Columns.Add(field.Name);
			}

			var result = new DbWriteCommand() 
			{
				InsertComamnd = sqlCmd,
				BulkCopy = sqlBulk,
				Table = dataTable
			};

			return result;
		}

		public void WriteObject(CollectTask task, object data)
		{
			if (data is VkList<long> && task.Method == "likes.getList")
			{
				writeObjectsLikes(task, data as VkList<long>);
			} else if (data is VkList<long>) {
				writeObjects(task, data as VkList<long>);
			}
			if (data is List<VkGroup>)
			{
				writeObjects(task, data as List<VkGroup>);
			}
			if (data is List<VkUser>)
			{
				writeObjects(task, data as List<VkUser>);
			}
			if (data is VkUserSubscriptions)
			{
				writeObjects(task, (data as VkUserSubscriptions).Groups);
			}
			if (data is VkList<VkPost>)
			{
				writeObjects(task, data as VkList<VkPost>);
			}
			if (data is VkList<VkComment>)
			{
				writeObjects(task, data as VkList<VkComment>);
			}

		}

		private void writeObjects(CollectTask task, VkList<VkComment> data)
		{
			var ids = task.Params.Split('_');
			var owner_id = long.Parse(ids[0]);
			var post_id = long.Parse(ids[1]);

			addDataRowDelegate addDataRow = (DataTable dt, object dat) =>
			{
				var d = dat as VkComment;
				dt.Rows.Add(owner_id, post_id, d.Id, d.FromId, d.Date, d.Text, d.Likes.Count);
			};
			setSqlParamsDelegate setSqlParam = (SqlCommand cmd, object dat) =>
			{
				var d = dat as VkComment;
				cmd.Parameters[0].Value = owner_id;
				cmd.Parameters[1].Value = post_id;
				cmd.Parameters[2].Value = d.Id;
				cmd.Parameters[3].Value = d.FromId;
				cmd.Parameters[4].Value = d.Date;
				cmd.Parameters[5].Value = d.Text;
				cmd.Parameters[6].Value = d.Likes.Count;
			};

			writeObjects(task, data.Items, setSqlParam, addDataRow);
		}
		
		private void writeObjects(CollectTask task, VkList<VkPost> data)
		{
			addDataRowDelegate addDataRow = (DataTable dt, object dat) =>
			{
				var d = dat as VkPost;
				var copyPost = d.CopyHistory == null ? new VkPost() : d.CopyHistory.FirstOrDefault();
				dt.Rows.Add(d.Id, d.FromId, d.OwnerId, d.Date, d.PostType.ToString(), d.Text, d.Comments.Count, d.Likes.Count, d.Reposts.Count, copyPost.Id, copyPost.FromId, copyPost.OwnerId, copyPost.Text ?? "");
			};
			setSqlParamsDelegate setSqlParam = (SqlCommand cmd, object dat) =>
			{
				var d = dat as VkPost;
				var copyPost = d.CopyHistory == null ? new VkPost() : d.CopyHistory.FirstOrDefault();
				cmd.Parameters[0].Value = d.Id;
				cmd.Parameters[1].Value = d.FromId;
				cmd.Parameters[2].Value = d.OwnerId;
				cmd.Parameters[3].Value = d.Date;
				cmd.Parameters[4].Value = d.PostType.ToString();
				cmd.Parameters[5].Value = d.Text;
				cmd.Parameters[6].Value = d.Comments.Count;
				cmd.Parameters[7].Value = d.Likes.Count;
				cmd.Parameters[8].Value = d.Reposts.Count;
				cmd.Parameters[9].Value = copyPost.Id;
				cmd.Parameters[10].Value = copyPost.FromId;
				cmd.Parameters[11].Value = copyPost.OwnerId;
				cmd.Parameters[12].Value = copyPost.Text ?? "";
			};

			writeObjects(task, data.Items, setSqlParam, addDataRow);
		}
		
		private void writeObjects(CollectTask task, List<VkUser> data)
		{
			addDataRowDelegate addDataRow = (DataTable dt, object dat) =>
			{
				var d = dat as VkUser;
				dt.Rows.Add(d.Id, d.FirstName, d.LastName, (int)d.Sex, d.Nickname, d.ScreenName, d.BDate, d.Country.Id, d.City.Id, d.Deactivated, d.Timezone, d.Photo50, d.Photo100, d.Photo200, d.PhotoMaxOrig, d.HasMobile, d.Online, d.MobilePhone, d.HomePhone, d.University, d.UniversityName, d.Faculty, d.FacultyName, d.Graduation);
			};

			setSqlParamsDelegate setSqlParam = (SqlCommand cmd, object dat) =>
			{
				var d = dat as VkUser;
				cmd.Parameters[0].Value = d.Id;
				cmd.Parameters[1].Value = d.FirstName;
				cmd.Parameters[2].Value = d.LastName;
				cmd.Parameters[3].Value = (int)d.Sex;
				cmd.Parameters[4].Value = d.Nickname;
				cmd.Parameters[5].Value = d.ScreenName;
				cmd.Parameters[6].Value = d.BDate;
				cmd.Parameters[7].Value = d.Country.Id;
				cmd.Parameters[8].Value = d.City.Id;
				cmd.Parameters[9].Value = d.Deactivated;
				cmd.Parameters[10].Value = d.Timezone;
				cmd.Parameters[11].Value = d.Photo50;
				cmd.Parameters[12].Value = d.Photo100;
				cmd.Parameters[13].Value = d.Photo200;
				cmd.Parameters[14].Value = d.PhotoMaxOrig;
				cmd.Parameters[15].Value = d.HasMobile;
				cmd.Parameters[16].Value = d.Online;
				cmd.Parameters[17].Value = d.MobilePhone;
				cmd.Parameters[18].Value = d.HomePhone;
				cmd.Parameters[19].Value = d.University;
				cmd.Parameters[20].Value = d.UniversityName;
				cmd.Parameters[21].Value = d.Faculty;
				cmd.Parameters[22].Value = d.FacultyName;
				cmd.Parameters[23].Value = d.Graduation;
			};

			writeObjects(task, data, setSqlParam, addDataRow);
		}

		private void writeObjects(CollectTask task, List<VkGroup> data)
		{
			addDataRowDelegate addDataRow = (DataTable dt, object dat) => {
				var d = dat as VkGroup;
				dt.Rows.Add(d.Id, d.Name, d.ScreenName, d.IsClosed, (int)d.Type, d.MembersCount, d.Photo50, d.Photo100, d.Photo200);
			};

			setSqlParamsDelegate setSqlParam = (SqlCommand cmd, object dat) => {
				var d = dat as VkGroup;
				cmd.Parameters[0].Value = d.Id;
				cmd.Parameters[1].Value = d.Name;
				cmd.Parameters[2].Value = d.ScreenName;
				cmd.Parameters[3].Value = d.IsClosed;
				cmd.Parameters[4].Value = (int)d.Type;
				cmd.Parameters[5].Value = d.MembersCount;
				cmd.Parameters[6].Value = d.Photo50;
				cmd.Parameters[7].Value = d.Photo100;
				cmd.Parameters[8].Value = d.Photo200;
			};

			writeObjects(task, data, setSqlParam, addDataRow);
		}

		protected void writeObjects(CollectTask task, VkList<long> data)
		{
			var p1 = long.Parse(task.Params);
			addDataRowDelegate addDataRow = (DataTable dt, object dat) =>
			{
				var d = (long)dat;
				dt.Rows.Add(p1, d);
			};
			setSqlParamsDelegate setSqlParam = (SqlCommand cmd, object dat) =>
			{
				var d = (long)dat;
				cmd.Parameters[0].Value = p1;
				cmd.Parameters[1].Value = d;
			};

			writeObjects(task, data.Items.Cast<object>(), setSqlParam, addDataRow);
		}

		protected void writeObjectsLikes(CollectTask task, VkList<long> data)
		{
			var ids = task.Params.Split('_'); // 1_45546_post => "owner_id", "item_id", "type"
			var owner_id = long.Parse(ids[0]);
			var item_id = long.Parse(ids[1]);
			var type = ids[2];

			addDataRowDelegate addDataRow = (DataTable dt, object dat) =>
			{
				var d = (long)dat;
				dt.Rows.Add(owner_id, item_id, type, d);
			};
			setSqlParamsDelegate setSqlParam = (SqlCommand cmd, object dat) =>
			{
				var d = (long)dat;
				cmd.Parameters[0].Value = owner_id;
				cmd.Parameters[1].Value = item_id;
				cmd.Parameters[2].Value = type;
				cmd.Parameters[3].Value = d;
			};

			writeObjects(task, data.Items.Cast<object>(), setSqlParam, addDataRow);
		}


		protected void writeObjects(CollectTask task, IEnumerable<object> items, setSqlParamsDelegate setSqlParams, addDataRowDelegate addDataRow)
		{
			checkConnection();

			// Create command to bulk copy and insert, as well as a DataTable
			var dbWriteCmds = createCommand(task);

			var insertCmd = dbWriteCmds.InsertComamnd;
			var bulkCopy = dbWriteCmds.BulkCopy;
			var dataTable = dbWriteCmds.Table;

			// Group items in batch
			var batchSize = 300;
			var groupedItems = items
				.Select((item, index) => new { Index = index, Item = item })
				.GroupBy(di => di.Index / batchSize)
				;

			// First, try to insert group in batch mode
			foreach (var item in groupedItems)
			{
				dataTable.Clear();
				foreach (var row in item)
				{
					addDataRow(dataTable, row.Item);
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
						setSqlParams(insertCmd, row.Item);

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
