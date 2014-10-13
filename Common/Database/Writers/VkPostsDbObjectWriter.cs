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
	public class VkPostsDbObjectWriter : BaseDbObjectWriter
	{
		public VkPostsDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{
			this.tableName = "posts";
		}

		protected override void setFieldsFromTask(CollectTask task)
		{

		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			var posts = data as VkList<VkPost>;
			return posts.Items;
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("id");
			table.Columns.Add("from_id");
			table.Columns.Add("to_id");
			table.Columns.Add("date");
			table.Columns.Add("type");
			table.Columns.Add("text");
			table.Columns.Add("comment_count");
			table.Columns.Add("like_count");
			table.Columns.Add("repost_count");
			table.Columns.Add("copy_id");
			table.Columns.Add("copy_from_id");
			table.Columns.Add("copy_to_id");
			table.Columns.Add("copy_text");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var post = item as VkPost;
			var copyPost = post.CopyHistory == null ? new VkPost() : post.CopyHistory.FirstOrDefault();

			var row = dataTable.NewRow();
			
			row["id"] = post.Id;
			row["from_id"] = post.FromId;
			row["to_id"] = post.OwnerId;
			row["date"] = post.Date;
			row["type"] = post.PostType.ToString();
			row["text"] = post.Text;
			row["comment_count"] = post.Comments.Count;
			row["like_count"] = post.Likes.Count;
			row["repost_count"] = post.Reposts.Count;
			row["copy_id"] = copyPost.Id;
			row["copy_from_id"] = copyPost.FromId;
			row["copy_to_id"] = copyPost.OwnerId;
			row["copy_text"] = copyPost.Text ?? "";

			dataTable.Rows.Add(row);
		}

		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var query = String.Format("SELECT id, to_id FROM {0} WHERE ", this.tableName);
			var whereTemplate = "(id = {0} AND to_id = {1})";

			var fullWhere = String.Join(" OR ",
				from i in items let p = i as VkPost select String.Format(whereTemplate, p.Id, p.OwnerId)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			var post = item as VkPost;

			return String.Format("{0}_{1}", post.Id, post.OwnerId);
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var id = reader.GetInt64(0);
			var ownerId = reader.GetInt64(1);

			return String.Format("{0}_{1}", id, ownerId);
		}
	}
}
