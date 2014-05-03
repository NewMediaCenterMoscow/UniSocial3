using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApiMapping
{
	interface IMethodMappingProvider
	{
		Dictionary<string, string> GetTypeMappings();
		string GetTypeMapping(string Method);
	}
}
