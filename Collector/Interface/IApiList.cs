using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Interface
{
	public interface IApiList<T>
	{
		long GetObjectCount();

		void AppendItems(IApiList<T> NewItems);

		T GetItems();
	}
}
