using Collector.Models.Vk;
using Common.Database.Writers;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database.Writers
{
	public class VkUserGroupsDbObjectWriter : BaseDbObjectWriter
	{
		protected long paramOne; // can by user_id or group_id depends on method
		protected bool isParamOneUserId;

		public VkUserGroupsDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{
			this.tableName = "user_groups";
		}

		protected override void setFieldsFromTask(CollectTask task)
		{
			this.paramOne = long.Parse(task.Params);
			if (task.Method == "groups.getMembers") // in list - user ids
			{
				this.isParamOneUserId = false;
			}
			if (task.Method == "groups.get" || task.Method == "users.getSubscriptions") // in list - group ids
			{
				this.isParamOneUserId = true;
			}

		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			if (data is VkList<long>)
				return (data as VkList<long>).Items.Cast<object>();
			else if (data is VkUserSubscriptions)
				return (data as VkUserSubscriptions).Groups.Items.Cast<object>();
			else
				throw new NotSupportedException(data.ToString());
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("user_id");
			table.Columns.Add("group_id");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var paramTwo = (long)item;

			var row = dataTable.NewRow();

			if (this.isParamOneUserId)
			{
				row["user_id"] = this.paramOne;
				row["group_id"] = paramTwo;
			}
			else
			{
				row["user_id"] = paramTwo;
				row["group_id"] = this.paramOne;
			}

			dataTable.Rows.Add(row);
		}

		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var whereTemplate = "";

			if (this.isParamOneUserId)
			{
				whereTemplate = String.Format("(user_id = {0} AND group_id = {{0}})", this.paramOne);
			}
			else
			{
				whereTemplate = String.Format("(user_id = {{0}} AND group_id = {0})", this.paramOne);
			}


			var query = String.Format("SELECT user_id, group_id FROM {0} WHERE ", this.tableName);

			var fullWhere = String.Join(" OR ",
				from i in items select String.Format(whereTemplate, (long)i)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			if (this.isParamOneUserId)
			{
				return String.Format("{0}_{1}", this.paramOne, (long)item);
			}
			else
			{
				return String.Format("{0}_{1}", (long)item, this.paramOne);
			}
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var userId = reader.GetInt64(0);
			var groupId = reader.GetInt64(1);

			return String.Format("{0}_{1}", userId, groupId);
		}
	}
}
