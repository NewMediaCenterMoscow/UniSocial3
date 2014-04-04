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
		public VkApi(IApiSettingsProvider ApiSettingsProvider)
			: base(new VkApiRequest(), ApiSettingsProvider)
		{
 
		}

		protected override void setListParams(NameValueCollection param, long offset, long count)
		{
			param.Set("offset", offset.ToString());
			param.Set("count", count.ToString());
		}
	}
}
