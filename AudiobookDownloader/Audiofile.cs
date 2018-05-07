using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class Audiofile
	{
		public int Id { get; set; }
		public int Chapter { get; set; }
		public string Name { get; set; }
		public string OwnerRecid { get; set; }
		public string AudiobookUrl { get; set; }
	}
}
