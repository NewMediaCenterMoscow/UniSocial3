using Collector.Api;
using Collector.Api.Settings;
using Collector.Interface;
using Common.ApiMapping;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.NinjectModules
{
	public class Vk : NinjectModule
	{
		string apiSettingsFilename = "Api/Settings/Vk.json";
		string mappingFilename = "ApiMapping/Vk.json";

		public override void Load()
		{
			Bind<IApiSettingsProvider>().To<ApiSettingsJsonFileProvider>().WithConstructorArgument<string>(apiSettingsFilename);
			Bind<IMethodMappingProvider>().To<VkMethodMappingProvider>().WithConstructorArgument<string>(mappingFilename);
			Bind<IApi>().To<VkApi>();
			Bind<IApiRequest>().To<VkApiRequest>();
		}
	}
}
