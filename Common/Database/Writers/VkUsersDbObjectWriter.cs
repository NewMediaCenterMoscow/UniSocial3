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
	public class VkUsersDbObjectWriter : BaseDbObjectWriter
	{
		public VkUsersDbObjectWriter(SqlConnection Conn)
			: base(Conn)
		{
			this.tableName = "users";
		}

		protected override void setFieldsFromTask(Model.CollectTask task)
		{

		}

		protected override IEnumerable<object> convertToObjectsList(object data)
		{
			var users = data as List<VkUser>;
			return users;
		}

		protected override DataTable createDataTable()
		{
			var table = new DataTable(this.tableName);

			table.Columns.Add("id");
			table.Columns.Add("first_name");
			table.Columns.Add("last_name");
			table.Columns.Add("sex");
			table.Columns.Add("nickname");
			table.Columns.Add("screen_name");
			table.Columns.Add("bdate");
			table.Columns.Add("country");
			table.Columns.Add("city");
			table.Columns.Add("deactivated");
			table.Columns.Add("timezone");
			table.Columns.Add("photo_50");
			table.Columns.Add("photo_100");
			table.Columns.Add("photo_200");
			table.Columns.Add("photo_max");
			table.Columns.Add("has_mobile");
			table.Columns.Add("online");
			table.Columns.Add("mobile_phone");
			table.Columns.Add("home_phone");
			table.Columns.Add("university");
			table.Columns.Add("university_name");
			table.Columns.Add("faculity");
			table.Columns.Add("faculity_name");
			table.Columns.Add("graduation");

			return table;
		}

		protected override void addItemToTable(DataTable dataTable, object item)
		{
			var user = item as VkUser;

			var row = dataTable.NewRow();

			row["id"] = user.Id;
			row["first_name"] = user.FirstName;
			row["last_name"] = user.LastName;
			row["sex"] = (int)user.Sex;
			row["nickname"] = user.Nickname;
			row["screen_name"] = user.ScreenName;
			row["bdate"] = user.BDate;
			row["deactivated"] = user.Deactivated;
			row["timezone"] = user.Timezone;
			row["photo_50"] = user.Photo50;
			row["photo_100"] = user.Photo100;
			row["photo_200"] = user.Photo200;
			row["photo_max"] = user.PhotoMaxOrig;
			row["has_mobile"] = user.HasMobile;
			row["online"] = user.Online;
			row["mobile_phone"] = user.MobilePhone;
			row["home_phone"] = user.HomePhone;
			row["university"] = user.University;
			row["university_name"] = user.UniversityName;
			row["faculity"] = user.Faculty;
			row["faculity_name"] = user.FacultyName;
			row["graduation"] = user.Graduation;

			if (user.Country != null)
				row["country"] = user.Country.Id;
			else
				row["country"] = null;

			if (user.City != null)
				row["city"] = user.City.Id;
			else
				row["city"] = null;

			dataTable.Rows.Add(row);
		}




		protected override string createExcludingItemsQuery(IEnumerable<object> items)
		{
			var query = String.Format("SELECT id FROM {0} WHERE ", this.tableName);
			var whereTemplate = "(id = {0})";

			var fullWhere = String.Join(" OR ",
				from i in items select String.Format(whereTemplate, (i as VkUser).Id)
			);
			var fullQuery = query + fullWhere;

			return fullQuery;
		}
		protected override string getItemKey(object item)
		{
			return (item as VkUser).Id.ToString();
		}
		protected override string getItemKey(SqlDataReader reader)
		{
			var id = reader.GetInt64(0);

			return id.ToString();
		}
	}
}
