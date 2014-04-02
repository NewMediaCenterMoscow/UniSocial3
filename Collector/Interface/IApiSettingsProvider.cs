using Collector.Api.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Interface
{
	public interface IApiSettingsProvider
	{
		ApiSettings GetSettingsForMethod(string Method);
	}
}
