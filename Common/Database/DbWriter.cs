using Collector.Models.Vk;
using Common.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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

		protected Dictionary<string, Tuple<string, SqlParameter[]>> commands;

		protected string settingsFilename = "Database/settings.json";

		public DbWriter(string ConnectionString)
		{
			connStr = ConnectionString;

			setConnection();

			commands = new Dictionary<string, Tuple<string, SqlParameter[]>>();
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

			var sqlParams = from ff in fields select new SqlParameter("@" + ff.Item1, ff.Item2);

			commands.Add(key, new Tuple<string, SqlParameter[]>(query, sqlParams.ToArray()));
		}



		//protected void setTableNamesAndCommands()
		//{
		//	tableNames = new Dictionary<string, string>();
		//	commands = new Dictionary<string, string>();

		//	addTableNameAndCommand(SocialNetwork.VKontakte, "groups.getMembers", "user_groups", "(group_id, user_id) VALUES (@p1, @p2)");
		//	addTableNameAndCommand(SocialNetwork.VKontakte, "groups.get", "user_groups", "(user_id, group_id) VALUES (@p1, @p2)");
		//}

		//private void addTableNameAndCommand(SocialNetwork network, string method, string tableName, string cmd)
		//{
		//	var key = ApiHelper.GetKey(network, method);
			
		//	tableNames.Add(key, tableName);

		//	var sql = "INSERT INTO " + tableNames[key] + " " + cmd;
		//	commands.Add(key, sql);
		//}

		public void WriteObject(CollectTask task, object data)
		{
			if (data is VkList<long>)
			{
				writeObject(task, data as VkList<long>);
			}
		}

		protected void writeObject(CollectTask task, VkList<long> data)
		{
			var key = ApiHelper.GetKey(task.SocialNetwork, task.Method);

			var query = commands[key].Item1;
			var sqlParams = commands[key].Item2;

			setConnection();

			var sqlCmd = new SqlCommand(query, sqlConn);
			sqlCmd.Parameters.AddRange(sqlParams);

			var p1 = long.Parse(task.Params);
			foreach (var item in data.Items)
			{
				sqlCmd.Parameters[0].Value = p1;
				sqlCmd.Parameters[1].Value = item;
				sqlCmd.ExecuteNonQuery();
			}
		}

		//protected void writeObject(CollectTask task, VkList<long> data)
		//{
		//	var key = ApiHelper.GetKey(task.SocialNetwork, task.Method);
		//	var tablename = tableNames[key];
		//	var query = commands[key];

		//	setConnection();

		//	var sqlCmd = new SqlCommand(query, sqlConn);
		//	var sqlParameters = new SqlParameter[] 
		//	{
		//	   new SqlParameter("@p1", SqlDbType.BigInt),
		//	   new SqlParameter("@p2", SqlDbType.BigInt),
		//	};
		//	sqlCmd.Parameters.AddRange(sqlParameters);

		//	var p1 = long.Parse(task.Params);
		//	foreach (var item in data.Items)
		//	{
		//		sqlCmd.Parameters[0].Value = p1;
		//		sqlCmd.Parameters[1].Value = item;
		//		sqlCmd.ExecuteNonQuery();
		//	}
		//}
		//protected void writeObject(CollectTask task, List<VkUser> data)
		//{
		//	var key = ApiHelper.GetKey(task.SocialNetwork, task.Method);
		//	var tablename = tableNames[key];
		//	var query = commands[key];

		//	setConnection();

		//}

	}
}
