using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class Grabber
	{
		private readonly IAudiobookService _service;
		private readonly OwnRadioClient _client = new OwnRadioClient();
		private const string _filename = "tmp.zip";

		public Grabber(IAudiobookService service)
		{
			_service = service;
		}

		public async Task Grab(Audiobook audiobook)
		{
			using (var fs = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite))
			{
				await _service.GetAudiobook(audiobook, fs);
			}

			Guid ownerRecId = Guid.NewGuid();

			using (var zip = ZipFile.OpenRead(_filename))
			{
				int chapter = 0;

				foreach (var entry in zip.Entries)
				{
					if (entry.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
					{
						using (var fs = entry.Open())
						{
							Guid recId = Guid.NewGuid();
							await _client.Upload(fs, ++chapter, entry.Name, ownerRecId, recId);
						}
					}
				}
			}
		}
	}
}
