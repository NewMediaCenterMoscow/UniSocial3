using Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database.Writers
{
	public abstract class BaseDbObjectWriter : IObjectWriter
	{
		protected SqlConnection sqlConn;

		protected long batchSize;

		protected string tableName;

		public BaseDbObjectWriter(SqlConnection Conn)
		{
			sqlConn = Conn;
			batchSize = 300;
		}

		public void WriteData(CollectTask task, object data)
		{
			setFieldsFromTask(task);

			var items = convertToObjectsList(data);

			if (!items.Any())
				return;
				
			// remove duplicates
			var uniqItems = items
				.GroupBy(o => getItemKey(o))
				.Select(g => g.First());

			// Group items for batch insert
			var groupedItems = uniqItems
				.Select((item, index) => new { Index = index, Item = item })
				.GroupBy(di => di.Index / batchSize , e => e.Item, (key, elem) => elem)
				//.Select(group => group.Select(gi => gi.Item))
				;

			var dataTable = createDataTable();

			// First, try to insert group in batch mode
			foreach (var group in groupedItems)
			{
				try
				{
					fillTableAndDoBulkCopy(group, dataTable);
				}
				catch (SqlException) // Exception - insert row-by-row
				{
					Trace.TraceWarning("Bulk exception: insert needed items");

					// Let's insert only needed items
					var itemsToInsert = excludeExistingItems(group);
					var dupItems = itemsToInsert.GroupBy(o => o.ToString()).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
					var dupGroup = group.GroupBy(o => o.ToString()).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

					if (itemsToInsert.Any())
					{
						fillTableAndDoBulkCopy(itemsToInsert, dataTable);
					}
					else
					{
						Trace.TraceWarning("Bulk exception: no needed items");
					}
				}
			}

		}

		protected void fillTableAndDoBulkCopy(IEnumerable<object> items, DataTable dataTable)
		{
			var bulkCopy = new SqlBulkCopy(this.sqlConn);
			bulkCopy.DestinationTableName = this.tableName;

			dataTable.Clear();
			foreach (var item in items)
			{
				addItemToTable(dataTable, item);
			}

			try
			{
				bulkCopy.WriteToServer(dataTable);
			}
			finally
			{
				bulkCopy.Close();					
			}
		}

		protected IEnumerable<object> excludeExistingItems(IEnumerable<object> items)
		{
			var query = createExcludingItemsQuery(items);
			var sqlCmd = new SqlCommand(query, this.sqlConn);
			var reader = sqlCmd.ExecuteReader();

			var existingItemsDict = new Dictionary<string, bool>();
			while (reader.Read())
			{
				var key = getItemKey(reader);
				existingItemsDict.Add(key, true);
			}
			reader.Close();

			var resultItems =
				from i in items
				let key = getItemKey(i)
				where !existingItemsDict.ContainsKey(key)
				select i;

			return resultItems;
		}


		protected abstract void setFieldsFromTask(CollectTask task);
		protected abstract IEnumerable<object> convertToObjectsList(object data);
		protected abstract DataTable createDataTable();
		protected abstract void addItemToTable(DataTable dataTable, object item);

		protected abstract string createExcludingItemsQuery(IEnumerable<object> items);
		protected abstract string getItemKey(object item);
		protected abstract string getItemKey(SqlDataReader reader);


	}
}
