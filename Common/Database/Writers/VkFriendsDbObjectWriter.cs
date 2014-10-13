using Collector.Models.Vk;
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
	public class VkFriendsDbObjectWriter : BaseDbObjectWriter
	{
		protected long userId;

		public VkFriendsDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{ 
			this.tableName = "friends";
		}

		protected override void setFieldsFromTask(CollectTask task)
		{
			this.userId = long.Parse(task.Params);
		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			var friendsList = data as VkList<long>;
			return friendsList.Items.Cast<object>();
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("user_id");
			table.Columns.Add("friend_id");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var friendId = (long)item;

			var row = dataTable.NewRow();

			row["user_id"] = this.userId;
			row["friend_id"] = friendId;

			dataTable.Rows.Add(row);
		}

		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var query = String.Format("SELECT user_id, friend_id FROM {0} WHERE ", this.tableName);
			var whereTemplate = String.Format("(user_id = {0} AND friend_id = {{0}})", this.userId);

			var fullWhere = String.Join(" OR ",
				from i in items select String.Format(whereTemplate, (long)i)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			return String.Format("{0}_{1}", this.userId, (long)item);
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var userId = reader.GetInt64(0);
			var friendId = reader.GetInt64(1);

			return String.Format("{0}_{1}", userId, friendId);
		}
	}
}
