using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdConrtoller
{
	public class FileCollectTask
	{
		public SocialNetwork SocialNetwork { get; set; }
		public string Method { get; set; }
		public string InputFilename { get; set; }
	}
}
