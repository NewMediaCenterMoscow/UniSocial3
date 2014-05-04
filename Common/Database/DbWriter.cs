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
							network = (SocialNetwork)Enum.Parse(typeof(SocialNetwork), (string)item["network"], true),
							method = (string)item["method"],
							table = (string)item["table"],
							fields = from prop in item["fields"] 
									 let jprop = prop as JProperty
									 select new Tuple<string, SqlDbType>(jprop.Name, (SqlDbType)Enum.Parse(typeof(SqlDbType), (string)jprop.Value, true))
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

			if (data is VkList<long>)
			{
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
		}

		protected delegate void addDataRowDelegate(DataTable dt, object data);
		protected delegate void setSqlParamsDelegate(SqlCommand cmd, object data);

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

		protected void writeObjects(CollectTask task, IEnumerable<object> items, setSqlParamsDelegate setSqlParams, addDataRowDelegate addDataRow)
		{
			// Create command to bulk copy and insert, as well as a DataTable
			var sqlCmds = createCommand(task);
			var insertCmd = sqlCmds.Item1;
			var bulkCopy = sqlCmds.Item2;
			var dataTable = sqlCmds.Item3;

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
