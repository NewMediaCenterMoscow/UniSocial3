﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Interface
{
	public interface IApi
	{
		Task<T> GetObject<T>(string Method, string Id);
		Task<T> GetObject<T>(string Method, List<string> Ids);
	}
}
