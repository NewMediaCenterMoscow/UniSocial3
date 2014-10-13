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
	public class VkCommentsDbObjectWriter : BaseDbObjectWriter
	{
		protected long ownerId;
		protected long postId;

		public VkCommentsDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{
			this.tableName = "comments";
		}

		protected override void setFieldsFromTask(CollectTask task)
		{
			var ids = task.Params.Split('_');
			this.ownerId = long.Parse(ids[0]);
			this.postId = long.Parse(ids[1]);
		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			var comments = data as VkList<VkComment>;
			return comments.Items;
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("owner_id");
			table.Columns.Add("post_id");
			table.Columns.Add("id");
			table.Columns.Add("from_id");
			table.Columns.Add("date");
			table.Columns.Add("text");
			table.Columns.Add("like_count");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var comment = item as VkComment;

			var row = dataTable.NewRow();

			row["owner_id"] = this.ownerId;
			row["post_id"] = this.postId;
			row["id"] = comment.Id;
			row["from_id"] = comment.FromId;
			row["date"] = comment.Date;
			row["text"] = comment.Text;
			row["like_count"] = comment.Likes.Count;

			dataTable.Rows.Add(row);
		}


		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var query = String.Format("SELECT owner_id, post_id, id FROM {0} WHERE ", this.tableName);
			var whereTemplate = String.Format("(owner_id = {0} AND post_id = {1} AND id = {{0}})", this.ownerId, this.postId);

			var fullWhere = String.Join(" OR ",
				from i in items select String.Format(whereTemplate, (i as VkComment).Id)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			return String.Format("{0}_{1}_{2}", this.ownerId, this.postId, (item as VkComment).Id);
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var ownerId = reader.GetInt64(0);
			var postId = reader.GetInt64(1);
			var id = reader.GetInt64(2);

			return String.Format("{0}_{1}_{2}", ownerId, postId, id);
		}
	}
}
