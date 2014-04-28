using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApiMapping
{
	public class VkMethodMappingProvider : BaseMethodMappingProvider
	{
		public VkMethodMappingProvider(string MappingFilename)
			: base(MappingFilename)
		{

		}

		protected override string transfomrType(string Name)
		{
			var leftBracket = Name.IndexOf('<');
			var rightBracket = Name.IndexOf('>');

			// Simple, just type
			if (leftBracket == -1)
			{
				return "Collector.Models.Vk." + Name + ", Collector";
			}

			var genericName = Name.Substring(0, leftBracket);
			var genericParameterName = Name.Substring(leftBracket + 1, rightBracket - leftBracket - 1);

			var fullGenericName = "";
			var fullGenericNameEnd = "";
			var fullGenericParameterName = "";

			if (genericName == "List")
			{
				fullGenericName = "System.Collections.Generic.List`1[";
				fullGenericNameEnd = "]";
			}
			else if (genericName == "VkList")
			{
				fullGenericName = "Collector.Models.Vk.VkList`1[";
				fullGenericNameEnd = "], Collector";
			}

			if (genericParameterName == "long")
			{
				fullGenericParameterName = "System.Int64";
			}
			else
			{
				fullGenericParameterName = "[Collector.Models.Vk." + genericParameterName + ", Collector]";
			}

			return fullGenericName + fullGenericParameterName + fullGenericNameEnd;
		}
	}
}
