using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database
{
	public interface IObjectWriter
	{
		void WriteData(CollectTask task, object data);
	}
}
