using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Common
{
	public static class DateTimeHelpers
	{
		static DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public static double ToUnixTimestamp(this DateTime value)
		{
			return (value - unixStart.ToLocalTime()).TotalSeconds;
		}

		public static DateTime FromUnixTimestamp(double UnixTimestamp)
		{
			return unixStart.AddSeconds(UnixTimestamp).ToLocalTime();
		}

	}
}
