using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.DatabaseContext
{
	class UploadAudiobook
	{
		public int Id { get; set; }
		public Audiobook Audiobook { get; set; }
	}
}
