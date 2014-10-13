using Collector.Models.Vk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database.Writers
{
	public class VkLikesDbObjectWriter : BaseDbObjectWriter
	{
		protected long ownerId;
		protected long itemId;
		protected string type;

		public VkLikesDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{ 
			this.tableName = "likes";
		}

		protected override void setFieldsFromTask(Model.CollectTask task)
		{
			var ids = task.Params.Split('_'); // 1_45546_post => "owner_id", "item_id", "type"
			this.ownerId = long.Parse(ids[0]);
			this.itemId = long.Parse(ids[1]);
			this.type = ids[2];
		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			var likersList = data as VkList<long>;
			return likersList.Items.Cast<object>();
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("owner_id");
			table.Columns.Add("item_id");
			table.Columns.Add("type");
			table.Columns.Add("user_id");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var userId = (long)item;

			var row = dataTable.NewRow();

			row["owner_id"] = this.ownerId;
			row["item_id"] = this.itemId;
			row["type"] = this.type;
			row["user_id"] = userId;

			dataTable.Rows.Add(row);
		}

		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var query = String.Format("SELECT owner_id, item_id, type, user_id FROM {0} WHERE ", this.tableName);
			var whereTemplate = String.Format(
				"(owner_id = {0} AND item_id = {1} AND type = '{2}' AND user_id = {{0}})", 
				this.ownerId, this.itemId, this.type
			);

			var fullWhere = String.Join(" OR ",
				from i in items select String.Format(whereTemplate, (long)i)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			return String.Format("{0}_{1}_{2}_{3}", this.ownerId, this.itemId, this.type, (long)item);
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var ownerId = reader.GetInt64(0);
			var itemId = reader.GetInt64(1);
			var type = reader.GetString(2);
			var userId = reader.GetInt64(3);

			return String.Format("{0}_{1}_{2}_{3}", ownerId, itemId, type, userId);
		}
	}
}
