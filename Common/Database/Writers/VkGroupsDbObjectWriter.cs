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
	public class VkGroupsDbObjectWriter : BaseDbObjectWriter
	{
		public VkGroupsDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{ 
			this.tableName = "groups";
		}

		protected override void setFieldsFromTask(CollectTask task)
		{

		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			var groups = data as List<VkGroup>;
			return groups;
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("id");
			table.Columns.Add("name");
			table.Columns.Add("screen_name");
			table.Columns.Add("is_closed");
			table.Columns.Add("type");
			table.Columns.Add("members_count");
			table.Columns.Add("photo_50");
			table.Columns.Add("photo_100");
			table.Columns.Add("photo_200");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var group = item as VkGroup;

			var row = dataTable.NewRow();

			row["id"] = group.Id;
			row["name"] = group.Name;
			row["screen_name"] = group.ScreenName;
			row["is_closed"] = group.IsClosed;
			row["type"] = (int)group.Type;
			row["members_count"] = group.MembersCount;
			row["photo_50"] = group.Photo50;
			row["photo_100"] = group.Photo100;
			row["photo_200"] = group.Photo200;

			dataTable.Rows.Add(row);
		}

		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var query = String.Format("SELECT id FROM {0} WHERE ", this.tableName);
			var whereTemplate = "(id = {0})";

			var fullWhere = String.Join(" OR ",
				from i in items select String.Format(whereTemplate, (i as VkGroup).Id)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			return (item as VkGroup).Id.ToString();
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var id = reader.GetInt64(0);

			return id.ToString();
		}
	}
}
