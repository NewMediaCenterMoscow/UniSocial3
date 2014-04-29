using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
	public enum SocialNetwork
	{
 		VKontakte,
		Facebook,
		Twitter
	}

	public class CollectTask
	{
		public SocialNetwork SocialNetwork { get; set; }
		public string Method { get; set; }
		public string Params { get; set; }

		public override string ToString()
		{
			return "[" + SocialNetwork + "] " + Method;
		}
	}
}
