using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.DatabaseContext
{
	class UploadAudiofile
	{
		public int Id { get; set; }
		public Audiofile File { get; set; }
	}
}
