using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class AudioBook
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string Title { get; set; }

		public List<AudioFile> Files { get; set; }
	}
}
