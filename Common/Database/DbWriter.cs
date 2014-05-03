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

		protected Dictionary<string, Tuple<string, List<Tuple<string,SqlDbType>>>> commands;

		protected string settingsFilename = "Database/settings.json";

		public DbWriter(string ConnectionString)
		{
			connStr = ConnectionString;

			setConnection();

			commands = new Dictionary<string,Tuple<string,List<Tuple<string,SqlDbType>>>>();
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
		}

		protected SqlCommand createCommand(CollectTask task)
		{
			var key = ApiHelper.GetKey(task.SocialNetwork, task.Method);

			var query = commands[key].Item1;
			var sqlParamsTuple = commands[key].Item2;

			var sqlParams = from ff in sqlParamsTuple select new SqlParameter("@" + ff.Item1, ff.Item2);

			var sqlCmd = new SqlCommand(query, sqlConn);
			sqlCmd.Parameters.AddRange(sqlParams.ToArray());

			return sqlCmd;
		}

		public void WriteObject(CollectTask task, object data)
		{
			checkConnection();

			var sqlCmd = createCommand(task);

			if (data is VkList<long>)
			{
				writeObjects(task, data as VkList<long>, sqlCmd);
			}
		}

		protected void writeObjects(CollectTask task, VkList<long> data, SqlCommand sqlCmd)
		{
			var p1 = long.Parse(task.Params);
			foreach (var item in data.Items)
			{
				sqlCmd.Parameters[0].Value = p1;
				sqlCmd.Parameters[1].Value = item;


				try
				{
					sqlCmd.ExecuteNonQuery();
				}
				catch (SqlException ex)
				{
					Trace.TraceError("SqlException: " + ex.Message);
				}
			}

			sqlCmd.Dispose();
		}



	}
}
