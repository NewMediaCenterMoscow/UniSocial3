using Collector.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Api
{
	public class VkApi : BaseApi
	{
		public VkApi(IApiSettingsProvider ApiSettingsProvider)
			: base(new VkApiRequest(), ApiSettingsProvider)
		{
 
		}


	}
}
