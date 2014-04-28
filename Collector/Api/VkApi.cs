using Collector.Interface;
using Collector.Models.Vk;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Api
{
	public class VkApi : BaseApi
	{
		public VkApi(IApiRequest ApiRequest, IApiSettingsProvider ApiSettingsProvider)
			: base(ApiRequest, ApiSettingsProvider)
		{
 
		}
	}
}
